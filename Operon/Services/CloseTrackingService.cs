using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.Services
{
    public sealed class CloseTrackingService : IDisposable
    {
        private const string DailyCloseEventsFilePrefix = "close-events-";

        private readonly object _gate = new object();
        private readonly System.Threading.Timer _heartbeatTimer;
        private readonly System.Threading.Timer _hangMonitorTimer;

        private DateTime _uiHeartbeatUtc;

        public Guid RunId { get; } = Guid.NewGuid();
        public DateTime StartUtc { get; } = DateTime.UtcNow;

        public bool IsUserInitiatedExit { get; set; }
        public bool ShutdownOrLogoffDetected { get; set; }
        public bool HangSuspected { get; private set; }

        public bool UnhandledUIExceptionDetected { get; private set; }
        public bool UnhandledBackgroundExceptionDetected { get; private set; }
        public bool UnobservedTaskExceptionDetected { get; private set; }

        public string? LastExceptionType { get; private set; }
        public string? LastExceptionMessage { get; private set; }
        public string? LastExceptionStackTrace { get; private set; }

        public int CrashLogRetentionDays { get; private set; } = AppConstants.Defaults.CrashLogRetentionDays;
        public int CrashLogMaxSizeMB { get; private set; } = AppConstants.Defaults.CrashLogMaxSizeMB;

        public CloseTrackingService()
        {
            _uiHeartbeatUtc = DateTime.UtcNow;

            TryDetectAndLogUnexpectedPreviousTermination();
            WriteLastRun(isRunning: true, endUtc: null, endType: null, closeReason: null);

            _heartbeatTimer = new System.Threading.Timer(_ => SafeUpdateHeartbeat(), null,
                TimeSpan.FromSeconds(AppConstants.Time.UiHeartbeatInitialDelaySeconds),
                TimeSpan.FromSeconds(AppConstants.Time.UiHeartbeatPeriodSeconds));
            _hangMonitorTimer = new System.Threading.Timer(_ => SafeCheckHang(), null,
                TimeSpan.FromSeconds(AppConstants.Time.HangMonitorInitialDelaySeconds),
                TimeSpan.FromSeconds(AppConstants.Time.HangMonitorPeriodSeconds));
        }

        public void ApplyCrashLogPolicy(int retentionDays, int maxSizeMb)
        {
            if (retentionDays < AppConstants.Limits.CrashRetentionDaysMin) retentionDays = AppConstants.Limits.CrashRetentionDaysMin;
            if (retentionDays > AppConstants.Limits.CrashRetentionDaysMax) retentionDays = AppConstants.Limits.CrashRetentionDaysMax;

            if (maxSizeMb < AppConstants.Limits.CrashMaxSizeMbMin) maxSizeMb = AppConstants.Limits.CrashMaxSizeMbMin;
            if (maxSizeMb > AppConstants.Limits.CrashMaxSizeMbMax) maxSizeMb = AppConstants.Limits.CrashMaxSizeMbMax;

            CrashLogRetentionDays = retentionDays;
            CrashLogMaxSizeMB = maxSizeMb;
        }

        public void CleanupCrashLogs()
        {
            try
            {
                string folder = AppPaths.GetLogsFolder();
                if (!Directory.Exists(folder))
                {
                    return;
                }

                var nowLocal = DateTime.Now.Date;
                var minDate = nowLocal.AddDays(-Math.Max(1, CrashLogRetentionDays) + 1);

                var files = Directory.GetFiles(folder, $"{DailyCloseEventsFilePrefix}*.jsonl", SearchOption.TopDirectoryOnly)
                                     .Select(p => new FileInfo(p))
                                     .OrderBy(f => f.CreationTimeUtc)
                                     .ToList();

                foreach (var file in files)
                {
                    var date = TryParseDailyLogDate(file.Name);
                    if (date.HasValue && date.Value.Date < minDate)
                    {
                        try { file.Delete(); } catch { }
                    }
                }

                EnforceMaxSize(folder);
            }
            catch
            {
            }
        }

        public void ClearCrashLogs()
        {
            try
            {
                string folder = AppPaths.GetLogsFolder();
                if (!Directory.Exists(folder))
                {
                    return;
                }

                foreach (var path in Directory.GetFiles(folder, $"{DailyCloseEventsFilePrefix}*.jsonl", SearchOption.TopDirectoryOnly))
                {
                    try { File.Delete(path); } catch { }
                }
            }
            catch
            {
            }
        }

        public void TouchUiHeartbeat()
        {
            lock (_gate)
            {
                _uiHeartbeatUtc = DateTime.UtcNow;
            }
        }

        public void MarkUnhandledUiException(Exception ex)
        {
            lock (_gate)
            {
                UnhandledUIExceptionDetected = true;
                SetException(ex);
            }
        }

        public void MarkUnhandledBackgroundException(Exception ex)
        {
            lock (_gate)
            {
                UnhandledBackgroundExceptionDetected = true;
                SetException(ex);
            }
        }

        public void MarkUnobservedTaskException(Exception ex)
        {
            lock (_gate)
            {
                UnobservedTaskExceptionDetected = true;
                SetException(ex);
            }
        }

        public void MarkHangSuspected()
        {
            lock (_gate)
            {
                HangSuspected = true;
            }
        }

        public void CompleteGracefulExit()
        {
            var reason = ComputeCloseReason();

            // Do not pollute crash history for valid closes. Only append problem cases.
            if (ShouldAppendToCrashLog(reason, IsUserInitiatedExit, ShutdownOrLogoffDetected))
            {
                AppendCloseEvent(reason, "GracefulExit");
                WriteLastRun(isRunning: false, endUtc: DateTime.UtcNow, endType: "Graceful", closeReason: reason.ToString());
            }
            else
            {
                // Still persist the real close reason to LastRun.json (e.g., UserInitiatedExit, ShutdownOrLogoff)
                // while keeping crash history clean.
                WriteLastRun(isRunning: false, endUtc: DateTime.UtcNow, endType: "Graceful", closeReason: reason.ToString());
            }
        }

        public void Dispose()
        {
            try { _heartbeatTimer.Dispose(); } catch { }
            try { _hangMonitorTimer.Dispose(); } catch { }
        }

        private void SafeUpdateHeartbeat()
        {
            try
            {
                WriteLastRun(isRunning: true, endUtc: null, endType: null, closeReason: null);
            }
            catch
            {
            }
        }

        private void SafeCheckHang()
        {
            try
            {
                DateTime lastUi;
                lock (_gate)
                {
                    lastUi = _uiHeartbeatUtc;
                }

                if (!HangSuspected && DateTime.UtcNow - lastUi > TimeSpan.FromSeconds(10))
                {
                    MarkHangSuspected();
                    if (ShouldAppendToCrashLog(CloseReason.HangSuspected, IsUserInitiatedExit, ShutdownOrLogoffDetected))
                    {
                        AppendCloseEvent(CloseReason.HangSuspected, "HangSuspected");
                    }
                }
            }
            catch
            {
            }
        }

        private static bool ShouldAppendToCrashLog(CloseReason reason, bool isUserInitiatedExit, bool shutdownOrLogoffDetected)
        {
            // Crash history log should include ONLY non-graceful/problem cases.
            // Exclude valid closes: Graceful, UserInitiatedExit, ShutdownOrLogoff.

            if (reason == CloseReason.Graceful)
            {
                return false;
            }

            if (reason == CloseReason.UserInitiatedExit || isUserInitiatedExit)
            {
                return false;
            }

            if (reason == CloseReason.ShutdownOrLogoff || shutdownOrLogoffDetected)
            {
                return false;
            }

            return reason == CloseReason.UnhandledUIException
                   || reason == CloseReason.UnhandledBackgroundException
                   || reason == CloseReason.UnobservedTaskException
                   || reason == CloseReason.HangSuspected
                   || reason == CloseReason.PreviousRunUnexpectedTermination
                   || reason == CloseReason.ProcessKilled
                   || reason == CloseReason.PowerLossOrHardShutdown;
        }

        private void SetException(Exception ex)
        {
            LastExceptionType = ex.GetType().FullName;
            LastExceptionMessage = ex.Message;
            LastExceptionStackTrace = ex.StackTrace;
        }

        private CloseReason ComputeCloseReason()
        {
            if (UnhandledUIExceptionDetected)
            {
                return CloseReason.UnhandledUIException;
            }

            if (UnhandledBackgroundExceptionDetected)
            {
                return CloseReason.UnhandledBackgroundException;
            }

            if (UnobservedTaskExceptionDetected)
            {
                return CloseReason.UnobservedTaskException;
            }

            if (HangSuspected)
            {
                return CloseReason.HangSuspected;
            }

            if (ShutdownOrLogoffDetected)
            {
                return CloseReason.ShutdownOrLogoff;
            }

            if (IsUserInitiatedExit)
            {
                return CloseReason.UserInitiatedExit;
            }

            return CloseReason.Graceful;
        }

        private void TryDetectAndLogUnexpectedPreviousTermination()
        {
            var previous = TryReadLastRun();
            if (previous == null)
            {
                return;
            }

            if (!previous.IsRunning)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var heartbeatAge = now - previous.LastHeartbeatUtc;

            var classification = heartbeatAge <= TimeSpan.FromSeconds(60)
                ? CloseReason.ProcessKilled
                : CloseReason.PowerLossOrHardShutdown;

            if (ShouldAppendToCrashLog(CloseReason.PreviousRunUnexpectedTermination, isUserInitiatedExit: false, shutdownOrLogoffDetected: false))
            {
                AppendCloseEvent(CloseReason.PreviousRunUnexpectedTermination, "PreviousRunUnexpectedTermination", previous, classification);
            }
        }

        private void AppendCloseEvent(CloseReason reason, string eventName, LastRunRecord? previousRun = null, CloseReason? previousClassification = null)
        {
            try
            {
                string path = GetDailyCloseEventsPath();

                var evt = new CloseEvent
                {
                    TimestampUtc = DateTime.UtcNow,
                    RunId = RunId,
                    StartUtc = StartUtc,
                    EventName = eventName,
                    CloseReason = reason.ToString(),
                    IsUserInitiatedExit = IsUserInitiatedExit,
                    ShutdownOrLogoffDetected = ShutdownOrLogoffDetected,
                    HangSuspected = HangSuspected,
                    ExceptionType = LastExceptionType,
                    ExceptionMessage = LastExceptionMessage,
                    ExceptionStackTrace = LastExceptionStackTrace,
                    PreviousRun = previousRun,
                    PreviousRunClassification = previousClassification?.ToString()
                };

                string json = JsonSerializer.Serialize(evt);
                File.AppendAllText(path, json + Environment.NewLine);
            }
            catch
            {
            }
        }

        private void WriteLastRun(bool isRunning, DateTime? endUtc, string? endType, string? closeReason)
        {
            try
            {
                var record = new LastRunRecord
                {
                    RunId = RunId,
                    StartUtc = StartUtc,
                    IsRunning = isRunning,
                    EndUtc = endUtc,
                    EndType = endType,
                    CloseReason = closeReason,
                    LastHeartbeatUtc = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(AppPaths.GetLastRunPath(), json);
            }
            catch
            {
            }
        }

        private LastRunRecord? TryReadLastRun()
        {
            try
            {
                string path = AppPaths.GetLastRunPath();
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

        private static string GetDailyCloseEventsPath()
        {
            var date = DateTime.Now.Date;
            return AppPaths.GetDailyCloseEventsPath(date);
        }

        private void EnforceMaxSize(string logsFolder)
        {
            try
            {
                long maxBytes = (long)Math.Max(1, CrashLogMaxSizeMB) * 1024L * 1024L;

                var files = Directory.GetFiles(logsFolder, $"{DailyCloseEventsFilePrefix}*.jsonl", SearchOption.TopDirectoryOnly)
                                     .Select(p => new FileInfo(p))
                                     .OrderBy(f => f.CreationTimeUtc)
                                     .ToList();

                long total = files.Sum(f => f.Exists ? f.Length : 0);
                foreach (var file in files)
                {
                    if (total <= maxBytes)
                    {
                        break;
                    }

                    try
                    {
                        long len = file.Exists ? file.Length : 0;
                        file.Delete();
                        total -= len;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private static DateTime? TryParseDailyLogDate(string fileName)
        {
            try
            {
                if (!fileName.StartsWith(DailyCloseEventsFilePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var part = fileName.Substring(DailyCloseEventsFilePrefix.Length);
                if (part.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
                {
                    part = part.Substring(0, part.Length - 5);
                }

                if (DateTime.TryParse(part, out var parsed))
                {
                    return parsed.Date;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

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
            public string EventName { get; set; } = string.Empty;
            public string CloseReason { get; set; } = string.Empty;

            public bool IsUserInitiatedExit { get; set; }
            public bool ShutdownOrLogoffDetected { get; set; }
            public bool HangSuspected { get; set; }

            public string? ExceptionType { get; set; }
            public string? ExceptionMessage { get; set; }
            public string? ExceptionStackTrace { get; set; }

            public LastRunRecord? PreviousRun { get; set; }
            public string? PreviousRunClassification { get; set; }
        }
    }
}
