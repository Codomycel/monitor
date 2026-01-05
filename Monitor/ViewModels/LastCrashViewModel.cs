using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace SystemActivityTracker.ViewModels
{
    public sealed class LastCrashViewModel : INotifyPropertyChanged
    {
        private const string AppFolderName = "SystemActivityTracker";
        private const string LastRunFileName = "LastRun.json";
        private const string CloseEventsFileName = "close-events.jsonl";
        private const string LogsFolderName = "logs";
        private const string DailyCloseEventsFilePrefix = "close-events-";

        private bool _hasCrashData;
        private string _crashSummary = "No crash data found.";
        private string _crashDetailsText = "No crash data found.";
        private string _crashLogPath = string.Empty;

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

        public LastCrashViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            OpenLogCommand = new RelayCommand(_ => OpenLog(), _ => !string.IsNullOrWhiteSpace(CrashLogPath) && File.Exists(CrashLogPath));
            Load();
        }

        public void Load()
        {
            try
            {
                var (evt, sourcePath) = TryReadLatestNonGracefulEvent();
                if (evt != null)
                {
                    CrashLogPath = sourcePath;
                    ApplyFromCloseEvent(evt);
                    return;
                }

                CrashLogPath = GetLegacyCloseEventsPath();

                var lastRun = TryReadLastRun();
                if (lastRun != null && !string.IsNullOrWhiteSpace(lastRun.CloseReason) && IsNonGraceful(lastRun.CloseReason))
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
            CrashSummary = "No crash data found.";
            CrashDetailsText = "No crash data found.";
        }

        private void ApplyFromCloseEvent(CloseEvent evt)
        {
            HasCrashData = true;

            var endLocal = evt.TimestampUtc.ToLocalTime();
            string reason = evt.CloseReason ?? "Unknown";
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

        private static CloseEvent? TryReadLatestNonGracefulEvent(string closeEventsPath)
        {
            try
            {
                if (!File.Exists(closeEventsPath))
                {
                    return null;
                }

                var lines = File.ReadLines(closeEventsPath).Reverse();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    CloseEvent? evt;
                    try
                    {
                        evt = JsonSerializer.Deserialize<CloseEvent>(line);
                    }
                    catch
                    {
                        continue;
                    }

                    if (evt == null)
                    {
                        continue;
                    }

                    if (IsNonGraceful(evt.CloseReason ?? string.Empty))
                    {
                        return evt;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static (CloseEvent? evt, string sourcePath) TryReadLatestNonGracefulEvent()
        {
            try
            {
                var logsFolder = GetLogsFolder();
                if (Directory.Exists(logsFolder))
                {
                    var files = Directory.GetFiles(logsFolder, $"{DailyCloseEventsFilePrefix}*.jsonl", SearchOption.TopDirectoryOnly)
                                         .OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase)
                                         .ToList();

                    foreach (var file in files)
                    {
                        var evt = TryReadLatestNonGracefulEvent(file);
                        if (evt != null)
                        {
                            return (evt, file);
                        }
                    }
                }

                var legacy = GetLegacyCloseEventsPath();
                var legacyEvt = TryReadLatestNonGracefulEvent(legacy);
                return (legacyEvt, legacy);
            }
            catch
            {
                var legacy = GetLegacyCloseEventsPath();
                return (null, legacy);
            }
        }

        private static LastRunRecord? TryReadLastRun()
        {
            try
            {
                string path = GetLastRunPath();
                if (!File.Exists(path))
                {
                    return null;
                }

                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<LastRunRecord>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string GetAppFolder()
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, AppFolderName);
            Directory.CreateDirectory(appFolder);
            return appFolder;
        }

        private static string GetLastRunPath() => Path.Combine(GetAppFolder(), LastRunFileName);

        private static string GetLogsFolder() => Path.Combine(GetAppFolder(), LogsFolderName);

        private static string GetLegacyCloseEventsPath() => Path.Combine(GetAppFolder(), CloseEventsFileName);

        private sealed class LastRunRecord
        {
            public Guid RunId { get; set; }
            public DateTime StartUtc { get; set; }
            public bool IsRunning { get; set; }
            public DateTime? EndUtc { get; set; }
            public string? EndType { get; set; }
            public string? CloseReason { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
        }

        private sealed class CloseEvent
        {
            public DateTime TimestampUtc { get; set; }
            public Guid RunId { get; set; }
            public DateTime StartUtc { get; set; }
            public string? EventName { get; set; }
            public string? CloseReason { get; set; }

            public bool IsUserInitiatedExit { get; set; }
            public bool ShutdownOrLogoffDetected { get; set; }
            public bool HangSuspected { get; set; }

            public string? ExceptionType { get; set; }
            public string? ExceptionMessage { get; set; }
            public string? ExceptionStackTrace { get; set; }

            public LastRunRecord? PreviousRun { get; set; }
            public string? PreviousRunClassification { get; set; }
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
