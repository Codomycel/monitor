using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using SystemActivityTracker.Utilities;
using SystemActivityTracker.Services;
using SystemActivityTracker.Services.Abstractions;

namespace SystemActivityTracker.ViewModels
{
    public sealed class LastCrashViewModel : INotifyPropertyChanged
    {
        private bool _hasCrashData;
        private string _crashSummary = GetString("CrashNoData", "No crash data found.");
        private string _crashDetailsText = GetString("CrashNoData", "No crash data found.");
        private string _crashLogPath = string.Empty;

        private readonly ICrashLogReader _crashLogReader;

        public bool HasCrashData
        {
            get => _hasCrashData;
            private set
            {
                if (_hasCrashData != value)
                {
                    _hasCrashData = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CrashSummary
        {
            get => _crashSummary;
            private set
            {
                if (!string.Equals(_crashSummary, value, StringComparison.Ordinal))
                {
                    _crashSummary = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CrashDetailsText
        {
            get => _crashDetailsText;
            private set
            {
                if (!string.Equals(_crashDetailsText, value, StringComparison.Ordinal))
                {
                    _crashDetailsText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CrashLogPath
        {
            get => _crashLogPath;
            private set
            {
                if (!string.Equals(_crashLogPath, value, StringComparison.Ordinal))
                {
                    _crashLogPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand OpenLogCommand { get; }

        public LastCrashViewModel(ICrashLogReader? crashLogReader = null)
        {
            _crashLogReader = crashLogReader ?? new CrashLogReader();
            RefreshCommand = new RelayCommand(_ => Load());
            OpenLogCommand = new RelayCommand(_ => OpenLog(), _ => !string.IsNullOrWhiteSpace(CrashLogPath));
            Load();
        }

        private static string GetString(string key, string fallback)
        {
            try
            {
                if (System.Windows.Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            catch
            {
            }

            return fallback;
        }

        public void Load()
        {
            try
            {
                if (_crashLogReader.TryReadLatestNonGracefulEvent(out var evt, out var sourcePath))
                {
                    CrashLogPath = sourcePath;
                    ApplyFromCloseEvent(evt);
                    return;
                }

                CrashLogPath = AppPaths.GetLegacyCloseEventsPath();

                if (_crashLogReader.TryReadLastRun(out var lastRun) && !string.IsNullOrWhiteSpace(lastRun.CloseReason) && IsNonGraceful(lastRun.CloseReason))
                {
                    ApplyFromLastRun(lastRun);
                    return;
                }

                SetNoCrashData();
            }
            catch
            {
                SetNoCrashData();
            }
            finally
            {
                if (OpenLogCommand is RelayCommand rc)
                {
                    rc.RaiseCanExecuteChanged();
                }
            }
        }

        private void SetNoCrashData()
        {
            HasCrashData = false;
            CrashSummary = GetString("CrashNoData", "No crash data found.");
            CrashDetailsText = GetString("CrashNoData", "No crash data found.");
        }

        private void ApplyFromCloseEvent(CrashLogEvent evt)
        {
            HasCrashData = true;

            var endLocal = evt.TimestampUtc.ToLocalTime();
            string reason = evt.CloseReason ?? GetString("Unknown", "Unknown");
            CrashSummary = $"{reason} at {endLocal:yyyy-MM-dd HH:mm:ss}";

            var sb = new StringBuilder();
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine($"End time (local): {endLocal:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"RunId: {evt.RunId}");
            sb.AppendLine($"App version: {GetAppVersion()}");

            if (evt.PreviousRun != null)
            {
                sb.AppendLine($"Previous RunId: {evt.PreviousRun.RunId}");
                sb.AppendLine($"Previous StartUtc: {evt.PreviousRun.StartUtc:O}");
                sb.AppendLine($"Previous LastHeartbeatUtc: {evt.PreviousRun.LastHeartbeatUtc:O}");
                if (!string.IsNullOrWhiteSpace(evt.PreviousRunClassification))
                {
                    sb.AppendLine($"Previous run classification: {evt.PreviousRunClassification}");
                }
            }

            if (evt.HangSuspected)
            {
                sb.AppendLine("Hang suspected: true");
            }

            sb.AppendLine($"Diagnostics log: {CrashLogPath}");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(evt.ExceptionType) || !string.IsNullOrWhiteSpace(evt.ExceptionMessage) || !string.IsNullOrWhiteSpace(evt.ExceptionStackTrace))
            {
                sb.AppendLine("Exception:");
                if (!string.IsNullOrWhiteSpace(evt.ExceptionType)) sb.AppendLine(evt.ExceptionType);
                if (!string.IsNullOrWhiteSpace(evt.ExceptionMessage)) sb.AppendLine(evt.ExceptionMessage);
                if (!string.IsNullOrWhiteSpace(evt.ExceptionStackTrace))
                {
                    sb.AppendLine();
                    sb.AppendLine(evt.ExceptionStackTrace);
                }
            }

            CrashDetailsText = sb.ToString().TrimEnd();
        }

        private void ApplyFromLastRun(LastRunRecord record)
        {
            HasCrashData = true;

            string reason = record.CloseReason ?? "Unknown";
            var endLocal = (record.EndUtc ?? record.LastHeartbeatUtc).ToLocalTime();
            CrashSummary = $"{reason} at {endLocal:yyyy-MM-dd HH:mm:ss}";

            var sb = new StringBuilder();
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine($"End time (local): {endLocal:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"RunId: {record.RunId}");
            sb.AppendLine($"App version: {GetAppVersion()}");
            sb.AppendLine($"LastHeartbeatUtc: {record.LastHeartbeatUtc:O}");
            sb.AppendLine($"Diagnostics log: {CrashLogPath}");

            CrashDetailsText = sb.ToString().TrimEnd();
        }

        private void OpenLog()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CrashLogPath))
                {
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = CrashLogPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
            }
        }

        private static string GetAppVersion()
        {
            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static bool IsNonGraceful(string closeReason)
        {
            if (string.IsNullOrWhiteSpace(closeReason))
            {
                return false;
            }

            return !string.Equals(closeReason, "Graceful", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(closeReason, "UserInitiatedExit", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(closeReason, "ShutdownOrLogoff", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

            public void Execute(object? parameter) => _execute(parameter);

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler? CanExecuteChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
