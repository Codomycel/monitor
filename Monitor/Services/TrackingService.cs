using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Microsoft.Win32;
using SystemActivityTracker.Models;

namespace SystemActivityTracker.Services
{
    public class TrackingService : IDisposable
    {
        private readonly SessionStateService _sessionStateService;
        private readonly ActivityLogWriter _logWriter = new ActivityLogWriter();
        private readonly System.Timers.Timer _timer;
        private TimeSpan _idleThreshold = TimeSpan.FromSeconds(300);
        private AppSettings _settings;
        private readonly object _syncRoot = new object();

        private DateTime _currentLogDate;

        private bool _isSuspended;
        private DateTime? _lastSystemEventTimestamp;

        private ActivityRecord? _currentRecord;
        private readonly List<ActivityRecord> _completedRecords = new List<ActivityRecord>();
        private bool _isRunning;
        private bool _isDisposed;

        public event EventHandler<ActivityRecord>? ActivityRecordCreated;
        public event EventHandler<DateTime>? DayRolledOver;

        public bool IsRunning => _isRunning;

        public TrackingService(SessionStateService sessionStateService, SettingsService? settingsService = null)
        {
            _sessionStateService = sessionStateService ?? throw new ArgumentNullException(nameof(sessionStateService));
            _sessionStateService.LockStateChanged += OnLockStateChanged;
            _sessionStateService.LockEvent += OnLockEvent;

            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            _currentLogDate = DateTime.Now.Date;

            // Load settings (or use defaults)
            _settings = settingsService?.Load() ?? new AppSettings();
            if (_settings.IdleThresholdMinutes <= 0)
            {
                _settings.IdleThresholdMinutes = 5;
            }

            if (_settings.PollIntervalSeconds <= 0)
            {
                _settings.PollIntervalSeconds = 5;
            }

            _idleThreshold = TimeSpan.FromMinutes(_settings.IdleThresholdMinutes);

            _timer = new System.Timers.Timer(_settings.PollIntervalSeconds * 1000);
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnLockStateChanged(object? sender, bool isLocked)
        {
            lock (_syncRoot)
            {
                if (!_isRunning)
                {
                    return;
                }

                UpdateActivityState();
            }
        }

        private void OnLockEvent(object? sender, SessionLockChangedEventArgs e)
        {
            lock (_syncRoot)
            {
                if (!_isRunning)
                {
                    return;
                }

                if (_isSuspended)
                {
                    LogSystemEvent("LOCK_EVENT_IGNORED_SUSPENDED", e.Timestamp);
                    return;
                }

                LogSystemEvent(e.IsLocked ? "LOCK" : "UNLOCK", e.Timestamp);
                HandleDayRolloverIfNeeded(e.Timestamp);
                SplitCurrentRecordAt(e.Timestamp, e.IsLocked, false);
            }
        }

        private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            lock (_syncRoot)
            {
                if (!_isRunning)
                {
                    return;
                }

                DateTime now = DateTime.Now;

                if (e.Mode == PowerModes.Suspend)
                {
                    LogSystemEvent("SUSPEND", now);
                    HandleDayRolloverIfNeeded(now);
                    FinalizeCurrentRecordAt(now);
                    _isSuspended = true;
                    _timer.Stop();
                    return;
                }

                if (e.Mode == PowerModes.Resume)
                {
                    LogSystemEvent("RESUME", now);
                    _isSuspended = false;
                    _currentLogDate = now.Date;
                    _timer.Start();
                    UpdateActivityState();
                }
            }
        }

        public void ApplySettings(AppSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (settings.IdleThresholdMinutes <= 0)
            {
                settings.IdleThresholdMinutes = 5;
            }
            if (settings.PollIntervalSeconds <= 0)
            {
                settings.PollIntervalSeconds = 5;
            }

            _settings = settings;
            _idleThreshold = TimeSpan.FromMinutes(_settings.IdleThresholdMinutes);

            _timer.Interval = _settings.PollIntervalSeconds * 1000;
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _timer.Stop();
            _isRunning = false;

            lock (_syncRoot)
            {
                DateTime now = DateTime.Now;
                HandleDayRolloverIfNeeded(now);
                FinalizeCurrentRecordAt(now);
                _currentRecord = null;
            }
        }

        public void FlushCurrentRecord()
        {
            lock (_syncRoot)
            {
                if (_currentRecord == null)
                {
                    return;
                }

                DateTime now = DateTime.Now;

                if (_isSuspended)
                {
                    LogSystemEvent("FLUSH_IGNORED_SUSPENDED", now);
                    return;
                }

                HandleDayRolloverIfNeeded(now);

                bool isLocked = _currentRecord.IsLocked;
                bool isIdle = _currentRecord.IsIdle;
                string processName = _currentRecord.ProcessName;
                string windowTitle = _currentRecord.WindowTitle;

                _currentRecord.EndTime = now;
                _completedRecords.Add(_currentRecord);
                _logWriter.AppendRecord(_currentRecord);
                ActivityRecordCreated?.Invoke(this, _currentRecord);

                _currentRecord = new ActivityRecord
                {
                    StartTime = now,
                    ProcessName = processName,
                    WindowTitle = windowTitle,
                    IsLocked = isLocked,
                    IsIdle = isIdle
                };
            }
        }

        public void Shutdown()
        {
            lock (_syncRoot)
            {
                if (_isRunning)
                {
                    _timer.Stop();
                    _isRunning = false;
                }

                DateTime now = DateTime.Now;
                LogSystemEvent("SHUTDOWN", now);
                HandleDayRolloverIfNeeded(now);
                FinalizeCurrentRecordAt(now);
                _currentRecord = null;
            }
        }

        public void HandleSessionEnding()
        {
            lock (_syncRoot)
            {
                DateTime now = DateTime.Now;
                LogSystemEvent("SESSION_ENDING", now);

                HandleDayRolloverIfNeeded(now);
                FinalizeCurrentRecordAt(now);
                _timer.Stop();
                _isRunning = false;
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_syncRoot)
            {
                UpdateActivityState();
            }
        }

        private void UpdateActivityState()
        {
            DateTime now = DateTime.Now;

            if (_isSuspended)
            {
                return;
            }

            HandleDayRolloverIfNeeded(now);

            bool isLocked = _sessionStateService.IsLocked;
            bool isIdle = false;
            string processName = string.Empty;
            string windowTitle = string.Empty;

            if (isLocked)
            {
                isIdle = false;
                processName = "LOCKED";
                windowTitle = string.Empty;
            }
            else
            {
                var idleTime = IdleTimeHelper.GetIdleTime();
                if (idleTime > _idleThreshold)
                {
                    isIdle = true;
                    processName = "IDLE";
                    windowTitle = string.Empty;
                }
                else
                {
                    isIdle = false;
                    if (!ActiveWindowHelper.TryGetActiveWindow(out processName, out windowTitle))
                    {
                        processName = string.Empty;
                        windowTitle = string.Empty;
                    }
                }
            }

            if (_currentRecord == null)
            {
                _currentLogDate = now.Date;
                _currentRecord = new ActivityRecord
                {
                    StartTime = now,
                    ProcessName = processName,
                    WindowTitle = windowTitle,
                    IsLocked = isLocked,
                    IsIdle = isIdle
                };

                return;
            }

            bool stateChanged =
                _currentRecord.IsLocked != isLocked ||
                _currentRecord.IsIdle != isIdle ||
                !string.Equals(_currentRecord.ProcessName, processName, StringComparison.Ordinal) ||
                !string.Equals(_currentRecord.WindowTitle, windowTitle, StringComparison.Ordinal);

            if (!stateChanged)
            {
                return;
            }

            _currentRecord.EndTime = now;
            _completedRecords.Add(_currentRecord);
            _logWriter.AppendRecord(_currentRecord);
            ActivityRecordCreated?.Invoke(this, _currentRecord);

            _currentRecord = new ActivityRecord
            {
                StartTime = now,
                ProcessName = processName,
                WindowTitle = windowTitle,
                IsLocked = isLocked,
                IsIdle = isIdle
            };

            _currentLogDate = now.Date;
        }

        private void HandleDayRolloverIfNeeded(DateTime now)
        {
            if (_currentLogDate == default)
            {
                _currentLogDate = now.Date;
            }

            if (_currentLogDate == now.Date)
            {
                return;
            }

            DateTime boundary = now.Date;

            if (_currentRecord != null && _currentRecord.StartTime < boundary)
            {
                var previousDaySegment = new ActivityRecord
                {
                    StartTime = _currentRecord.StartTime,
                    EndTime = boundary,
                    ProcessName = _currentRecord.ProcessName,
                    WindowTitle = _currentRecord.WindowTitle,
                    IsLocked = _currentRecord.IsLocked,
                    IsIdle = _currentRecord.IsIdle
                };

                _completedRecords.Add(previousDaySegment);
                _logWriter.AppendRecord(previousDaySegment);
                ActivityRecordCreated?.Invoke(this, previousDaySegment);

                _currentRecord = new ActivityRecord
                {
                    StartTime = boundary,
                    ProcessName = previousDaySegment.ProcessName,
                    WindowTitle = previousDaySegment.WindowTitle,
                    IsLocked = previousDaySegment.IsLocked,
                    IsIdle = previousDaySegment.IsIdle
                };
            }
            else
            {
                _currentRecord = new ActivityRecord
                {
                    StartTime = boundary,
                    ProcessName = string.Empty,
                    WindowTitle = string.Empty,
                    IsLocked = _sessionStateService.IsLocked,
                    IsIdle = false
                };
            }

            _currentLogDate = now.Date;
            DayRolledOver?.Invoke(this, _currentLogDate);
        }

        private void FinalizeCurrentRecordAt(DateTime timestamp)
        {
            if (_currentRecord == null)
            {
                return;
            }

            if (_currentRecord.EndTime != null)
            {
                return;
            }

            if (timestamp < _currentRecord.StartTime)
            {
                timestamp = _currentRecord.StartTime;
            }

            _currentRecord.EndTime = timestamp;
            _completedRecords.Add(_currentRecord);
            _logWriter.AppendRecord(_currentRecord);
            ActivityRecordCreated?.Invoke(this, _currentRecord);
        }

        private void SplitCurrentRecordAt(DateTime timestamp, bool targetIsLocked, bool targetIsIdle)
        {
            if (_currentRecord == null)
            {
                _currentRecord = new ActivityRecord
                {
                    StartTime = timestamp,
                    ProcessName = targetIsLocked ? "LOCKED" : (targetIsIdle ? "IDLE" : string.Empty),
                    WindowTitle = string.Empty,
                    IsLocked = targetIsLocked,
                    IsIdle = targetIsIdle
                };

                _currentLogDate = timestamp.Date;
                return;
            }

            if (timestamp < _currentRecord.StartTime)
            {
                timestamp = _currentRecord.StartTime;
            }

            if (_currentRecord.EndTime == null)
            {
                _currentRecord.EndTime = timestamp;
                _completedRecords.Add(_currentRecord);
                _logWriter.AppendRecord(_currentRecord);
                ActivityRecordCreated?.Invoke(this, _currentRecord);
            }

            _currentRecord = new ActivityRecord
            {
                StartTime = timestamp,
                ProcessName = targetIsLocked ? "LOCKED" : (targetIsIdle ? "IDLE" : string.Empty),
                WindowTitle = string.Empty,
                IsLocked = targetIsLocked,
                IsIdle = targetIsIdle
            };

            _currentLogDate = timestamp.Date;
        }

        private void LogSystemEvent(string name, DateTime timestamp)
        {
            double deltaSeconds = 0;
            if (_lastSystemEventTimestamp.HasValue)
            {
                deltaSeconds = (timestamp - _lastSystemEventTimestamp.Value).TotalSeconds;
            }

            _lastSystemEventTimestamp = timestamp;
            Debug.WriteLine($"[Tracking] {name} at {timestamp:o} (Δ{deltaSeconds:0}s)");
        }

#if DEBUG
        internal void DebugSimulateDayRollover(DateTime now)
        {
            lock (_syncRoot)
            {
                HandleDayRolloverIfNeeded(now);
            }
        }
#endif

        public IReadOnlyList<ActivityRecord> GetCompletedRecords()
        {
            lock (_syncRoot)
            {
                return _completedRecords.AsReadOnly();
            }
        }

        public bool TryGetCurrentStateSnapshot(out bool isRunning, out DateTime? currentRecordStartTime, out bool isLocked, out bool isIdle)
        {
            lock (_syncRoot)
            {
                isRunning = _isRunning;
                if (_currentRecord == null)
                {
                    currentRecordStartTime = null;
                    isLocked = false;
                    isIdle = false;
                    return false;
                }

                currentRecordStartTime = _currentRecord.StartTime;
                isLocked = _currentRecord.IsLocked;
                isIdle = _currentRecord.IsIdle;
                return true;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _sessionStateService.LockStateChanged -= OnLockStateChanged;
            _sessionStateService.LockEvent -= OnLockEvent;

            SystemEvents.PowerModeChanged -= OnPowerModeChanged;

            Shutdown();
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        ~TrackingService()
        {
            Dispose();
        }
    }
}
