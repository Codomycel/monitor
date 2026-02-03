using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using SystemActivityTracker.Models;
using SystemActivityTracker.Services;
using SystemActivityTracker.Services.Abstractions;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly TrackingService? _trackingService;
        private readonly SettingsService? _settingsService;
        private readonly IActivityLogReader _activityLogReader;
        private readonly ManualTaskService _manualTaskService;
        private string _trackingStatus = GetString("TrackingStatusStopped", "Tracking status: Stopped");
        private TimeSpan _totalActiveTimeToday;
        private TimeSpan _totalIdleTimeToday;
        private TimeSpan _totalLockedTimeToday;
        private readonly ObservableCollection<AppUsageSummary> _todayAppUsage = new ObservableCollection<AppUsageSummary>();
        private readonly ObservableCollection<ManualTaskEntry> _manualTasks = new ObservableCollection<ManualTaskEntry>();
        private ManualTaskEntry? _selectedManualTask;
        private bool _isManualEditMode;
        private string _manualTaskName = string.Empty;
        private string _manualHours = string.Empty;
        private string _manualMinutes = string.Empty;
        private string _manualSeconds = string.Empty;
        private int _selectedTabIndex;
        private int _idleThresholdMinutes;
        private int _pollIntervalSeconds;
        private bool _enableLiveRefresh;
        private int _liveRefreshIntervalSeconds;
        private bool _isTestMode;
        private int _crashLogRetentionDays;
        private int _crashLogMaxSizeMB;
        private DateTime _selectedDate = DateTime.Today;
        private DateTime _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _weekStartDate;
        private readonly ObservableCollection<DailySummary> _weeklySummaries = new ObservableCollection<DailySummary>();
        private readonly ObservableCollection<MonthlyAppUsageDto> _monthlyAppUsage = new ObservableCollection<MonthlyAppUsageDto>();
        private bool _isMonthlyUsageEmpty = true;
        private TimeSpan _weeklyTrackedActiveDuration;
        private TimeSpan _weeklyManualDuration;
        private TimeSpan _weeklyTotalActiveDuration;
        private TimeSpan _weeklyTotalIdleDuration;
        private TimeSpan _weeklyTotalLockedDuration;
        private DateTime? _selectedDayStartTime;
        private DateTime? _selectedDayEndTime;
        private TimeSpan _selectedDayManualDuration;
        private DateTime? _runStartUtc;
        private TimeSpan _accumulatedRunTime = TimeSpan.Zero;
        private readonly DispatcherTimer _runningTimer = new DispatcherTimer();
        private int _lastDisplayedRunSecond = -1;
        private string _headerRunningTimerText = "00:00:00";
        private TimeSpan _headerActiveBase = TimeSpan.Zero;
        private DateTime? _headerActiveStartLocal;
        private DateTime? _headerActiveLastRecordStartLocal;
        private int _lastDisplayedActiveSecond = -1;
        private string _headerActiveTimerText = "00:00:00";
        private readonly DispatcherTimer _autoRefreshTimer = new DispatcherTimer();
        private AppSettings _settingsSnapshot = new AppSettings();

        public LastCrashViewModel LastCrash { get; }

        public MainWindowViewModel(
            TrackingService? trackingService,
            SettingsService? settingsService = null,
            IActivityLogReader? activityLogReader = null,
            ManualTaskService? manualTaskService = null,
            LastCrashViewModel? lastCrashViewModel = null)
        {
            _trackingService = trackingService;
            _settingsService = settingsService;
            _activityLogReader = activityLogReader ?? new ActivityLogReader();
            _manualTaskService = manualTaskService ?? new ManualTaskService();

            LastCrash = lastCrashViewModel ?? new LastCrashViewModel();
            TodayText = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            _weekStartDate = StartOfWeek(DateTime.Today, DayOfWeek.Monday);

            _runningTimer.Interval = TimeSpan.FromMilliseconds(100);
            _runningTimer.Tick += (_, __) =>
            {
                TickRunningTimer();
                TickHeaderActiveTimer();
            };
            StartCommand = new RelayCommand(_ => StartTracking());

            StopCommand = new RelayCommand(_ => StopTracking());

            RefreshCommand = new RelayCommand(_ => RefreshForSelectedDate());

            PrimaryManualTaskCommand = new RelayCommand(_ => PrimaryManualTaskAction(), _ => CanEditManualTasks());
            BeginEditManualTaskCommand = new RelayCommand(p => BeginEditManualTask(p as ManualTaskEntry), p => CanEditManualTasks() && !_isManualEditMode);
            CancelManualTaskEditCommand = new RelayCommand(_ => CancelManualTaskEdit(), _ => CanEditManualTasks() && _isManualEditMode);
            DeleteManualTaskRowCommand = new RelayCommand(p => DeleteManualTaskRow(p as ManualTaskEntry), p => CanEditManualTasks() && !_isManualEditMode);

            LoadMonthlyUsageCommand = new RelayCommand(_ => LoadMonthlyUsage());

            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            ClearCrashLogsCommand = new RelayCommand(_ => ClearCrashLogs());
            LoadWeeklyCommand = new RelayCommand(_ => LoadWeeklySummary());
            ForceWriteNowCommand = new RelayCommand(_ =>
            {
                _trackingService?.FlushCurrentRecord();
                RefreshSelectedDaySummary();
                RefreshSelectedDayAppUsage();
                RefreshWeeklySummary();
            });

            _autoRefreshTimer.Tick += (_, __) =>
            {
                if (_trackingService != null && _trackingService.IsRunning && SelectedDate.Date == DateTime.Today)
                {
                    RefreshCommand.Execute(null);
                }
            };

            if (_trackingService != null)
            {
                _trackingService.DayRolledOver += (_, newDate) =>
                {
                    var dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher == null)
                    {
                        return;
                    }

                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        var previousToday = newDate.Date.AddDays(-1);
                        if (SelectedDate.Date == previousToday)
                        {
                            SelectedDate = newDate.Date;
                        }

                        RefreshCommand.Execute(null);
                    }));
                };
            }

            // Load settings for UI
            var settings = _settingsService?.Load() ?? new AppSettings();
            if (settings.IdleThresholdMinutes <= 0) settings.IdleThresholdMinutes = 5;
            if (settings.PollIntervalSeconds <= 0) settings.PollIntervalSeconds = 5;
            if (settings.LiveRefreshIntervalSeconds <= 0) settings.LiveRefreshIntervalSeconds = 30;

            // Crash log policy clamps
            if (settings.CrashLogRetentionDays < 1) settings.CrashLogRetentionDays = 14;
            if (settings.CrashLogMaxSizeMB < 1) settings.CrashLogMaxSizeMB = 50;

            _settingsSnapshot = settings;

            IdleThresholdMinutes = settings.IdleThresholdMinutes;
            PollIntervalSeconds = settings.PollIntervalSeconds;
            EnableLiveRefresh = settings.EnableLiveRefresh;
            LiveRefreshIntervalSeconds = settings.LiveRefreshIntervalSeconds;
            CrashLogRetentionDays = settings.CrashLogRetentionDays;
            CrashLogMaxSizeMB = settings.CrashLogMaxSizeMB;

            if (settings.AutoStartTrackingOnLaunch)
            {
                StartTracking();
            }

            LoadWeeklySummary();

            LoadMonthlyUsage();

            RefreshForSelectedDate();

            SyncHeaderActiveBaseFromSummary();

            LoadManualTasksForSelectedDate();
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

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDayDetailsMode));
                    UpdateManualTaskCommandsCanExecute();

                    if (IsDayDetailsMode)
                    {
                        LoadManualTasksForSelectedDate();
                    }
                    else
                    {
                        CancelManualTaskEdit();
                    }
                }
            }
        }

        public bool IsDayDetailsMode => SelectedTabIndex == 0;

        public bool IsManualEditMode
        {
            get => _isManualEditMode;
            private set
            {
                if (_isManualEditMode != value)
                {
                    _isManualEditMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PrimaryManualTaskGlyph));
                    OnPropertyChanged(nameof(ManualRowActionGlyph));
                    UpdateManualTaskCommandsCanExecute();
                }
            }
        }

        private void StartTracking()
        {
            if (_trackingService == null)
            {
                return;
            }

            if (_trackingService.IsRunning)
            {
                return;
            }

            _trackingService.Start();

            ResetHeaderActiveTickerForNewRun();

            TrackingStatus = GetString("TrackingStatusRunning", "Tracking status: Running");
            OnPropertyChanged(nameof(IsTrackingRunning));
            StartRunningTimerTicker();
            ApplyLiveRefreshSettings();
        }

        private void StopTracking()
        {
            if (_trackingService == null)
            {
                return;
            }

            if (!_trackingService.IsRunning)
            {
                TrackingStatus = GetString("TrackingStatusStopped", "Tracking status: Stopped");
                OnPropertyChanged(nameof(IsTrackingRunning));
                StopRunningTimerTicker();
                _autoRefreshTimer.Stop();
                return;
            }

            _trackingService.Stop();
            TrackingStatus = GetString("TrackingStatusStopped", "Tracking status: Stopped");
            OnPropertyChanged(nameof(IsTrackingRunning));
            StopRunningTimerTicker();
            ApplyLiveRefreshSettings();
        }

        private void ResetHeaderActiveTickerForNewRun()
        {
            // UI-only reset so Stop -> Start doesn't reuse a stale start timestamp from a previous run.
            // Keep the base as the already-recorded total active for today; the live delta starts fresh.
            _headerActiveBase = ComputeActiveTotalForDate(DateTime.Today);

            _headerActiveStartLocal = DateTime.Now;
            _headerActiveLastRecordStartLocal = null;

            _lastDisplayedActiveSecond = -1;
            TickHeaderActiveTimer();
        }

        public string HeaderRunningTimerText
        {
            get => _headerRunningTimerText;
            private set
            {
                if (!string.Equals(_headerRunningTimerText, value, StringComparison.Ordinal))
                {
                    _headerRunningTimerText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HeaderActiveTimerText
        {
            get => _headerActiveTimerText;
            private set
            {
                if (!string.Equals(_headerActiveTimerText, value, StringComparison.Ordinal))
                {
                    _headerActiveTimerText = value;
                    OnPropertyChanged();
                }
            }
        }

        private void StartRunningTimerTicker()
        {
            if (_runStartUtc.HasValue)
            {
                return;
            }

            _runStartUtc = DateTime.UtcNow;
            _lastDisplayedRunSecond = -1;
            TickRunningTimer();
            _runningTimer.Start();
        }

        private void StopRunningTimerTicker()
        {
            if (_runStartUtc.HasValue)
            {
                _accumulatedRunTime += DateTime.UtcNow - _runStartUtc.Value;
                _runStartUtc = null;
            }

            _runningTimer.Stop();
            TickRunningTimer();
            TickHeaderActiveTimer();
        }

        private void TickRunningTimer()
        {
            DateTime now = DateTime.Now;

            TrackingService.TrackingSnapshot snapshot = default;
            bool hasSnapshot = _trackingService != null && _trackingService.TryGetSnapshot(out snapshot);
            bool isActive = hasSnapshot && !snapshot.IsLocked && !snapshot.IsIdle;

            if (isActive)
            {
                if (_headerActiveStartLocal == null)
                {
                    var start = snapshot.CurrentRecordStartTime ?? now;
                    if (start > now)
                    {
                        start = now;
                    }

                    _headerActiveStartLocal = start;
                    _headerActiveLastRecordStartLocal = snapshot.CurrentRecordStartTime;
                }
                else if (snapshot.CurrentRecordStartTime.HasValue && _headerActiveLastRecordStartLocal.HasValue && snapshot.CurrentRecordStartTime.Value != _headerActiveLastRecordStartLocal.Value)
                {
                    var delta = now - _headerActiveStartLocal.Value;
                    if (delta < TimeSpan.Zero)
                    {
                        delta = TimeSpan.Zero;
                    }

                    _headerActiveBase += delta;

                    var start = snapshot.CurrentRecordStartTime.Value;
                    if (start > now)
                    {
                        start = now;
                    }

                    _headerActiveStartLocal = start;
                    _headerActiveLastRecordStartLocal = snapshot.CurrentRecordStartTime.Value;
                }
            }
            else
            {
                if (_headerActiveStartLocal.HasValue)
                {
                    var delta = now - _headerActiveStartLocal.Value;
                    if (delta < TimeSpan.Zero)
                    {
                        delta = TimeSpan.Zero;
                    }

                    _headerActiveBase += delta;
                    _headerActiveStartLocal = null;
                    _headerActiveLastRecordStartLocal = null;
                }
            }

            var total = _headerActiveBase;
            if (_headerActiveStartLocal.HasValue)
            {
                var delta = now - _headerActiveStartLocal.Value;
                if (delta < TimeSpan.Zero)
                {
                    delta = TimeSpan.Zero;
                }

                total += delta;
            }

            if (total < TimeSpan.Zero)
            {
                total = TimeSpan.Zero;
            }

            int wholeSeconds = (int)total.TotalSeconds;
            if (wholeSeconds == _lastDisplayedRunSecond)
            {
                return;
            }

            _lastDisplayedRunSecond = wholeSeconds;

            int hours = (int)total.TotalHours;
            int minutes = total.Minutes;
            int seconds = total.Seconds;
            HeaderRunningTimerText = GetString("HeaderActivePrefix", "Active - ") + string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        private void SyncHeaderActiveBaseFromSummary()
        {
            _headerActiveBase = ComputeActiveTotalForDate(DateTime.Today);
            _headerActiveStartLocal = null;
            _headerActiveLastRecordStartLocal = null;
            _lastDisplayedActiveSecond = -1;
            TickHeaderActiveTimer();
        }

        private TimeSpan ComputeActiveTotalForDate(DateTime date)
        {
            TimeSpan totalActive = TimeSpan.Zero;

            if (!_activityLogReader.TryReadDay(date.Date, out var entries))
            {
                return TimeSpan.Zero;
            }

            foreach (var entry in entries)
            {
                if (!entry.IsLocked && !entry.IsIdle)
                {
                    totalActive += entry.EndTime - entry.StartTime;
                }
            }

            return totalActive;
        }

        private void TickHeaderActiveTimer()
        {
            DateTime now = DateTime.Now;

            TrackingService.TrackingSnapshot snapshot = default;
            bool hasSnapshot = _trackingService != null && _trackingService.TryGetSnapshot(out snapshot);
            bool isActive = hasSnapshot && !snapshot.IsLocked && !snapshot.IsIdle;

            if (isActive)
            {
                if (_headerActiveStartLocal == null)
                {
                    var start = snapshot.CurrentRecordStartTime ?? now;
                    if (start > now)
                    {
                        start = now;
                    }

                    _headerActiveStartLocal = start;
                    _headerActiveLastRecordStartLocal = snapshot.CurrentRecordStartTime;
                }
                else if (snapshot.CurrentRecordStartTime.HasValue && _headerActiveLastRecordStartLocal.HasValue && snapshot.CurrentRecordStartTime.Value != _headerActiveLastRecordStartLocal.Value)
                {
                    var delta = now - _headerActiveStartLocal.Value;
                    if (delta < TimeSpan.Zero)
                    {
                        delta = TimeSpan.Zero;
                    }

                    _headerActiveBase += delta;

                    var start = snapshot.CurrentRecordStartTime.Value;
                    if (start > now)
                    {
                        start = now;
                    }

                    _headerActiveStartLocal = start;
                    _headerActiveLastRecordStartLocal = snapshot.CurrentRecordStartTime.Value;
                }
            }
            else
            {
                if (_headerActiveStartLocal.HasValue)
                {
                    var delta = now - _headerActiveStartLocal.Value;
                    if (delta < TimeSpan.Zero)
                    {
                        delta = TimeSpan.Zero;
                    }

                    _headerActiveBase += delta;
                    _headerActiveStartLocal = null;
                    _headerActiveLastRecordStartLocal = null;
                }
            }

            var total = _headerActiveBase;
            if (_headerActiveStartLocal.HasValue)
            {
                var delta = now - _headerActiveStartLocal.Value;
                if (delta < TimeSpan.Zero)
                {
                    delta = TimeSpan.Zero;
                }

                total += delta;
            }

            if (total < TimeSpan.Zero)
            {
                total = TimeSpan.Zero;
            }

            int wholeSeconds = (int)total.TotalSeconds;
            if (wholeSeconds == _lastDisplayedActiveSecond)
            {
                return;
            }

            _lastDisplayedActiveSecond = wholeSeconds;

            int hours = (int)total.TotalHours;
            int minutes = total.Minutes;
            int seconds = total.Seconds;
            HeaderActiveTimerText = GetString("HeaderTotalActivePrefix", "Total active - ") + string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        public string TodayText { get; }

        public string TrackingStatus
        {
            get => _trackingStatus;
            set
            {
                if (_trackingStatus != value)
                {
                    _trackingStatus = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsTrackingRunning));
                }
            }
        }

        public bool IsTrackingRunning => _trackingService != null && _trackingService.IsRunning;

        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                var normalized = value == default
                    ? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                    : new DateTime(value.Year, value.Month, 1);

                if (_selectedMonth != normalized)
                {
                    _selectedMonth = normalized;
                    OnPropertyChanged();
                    LoadMonthlyUsage();
                }
            }
        }

        public bool IsTestMode
        {
            get => _isTestMode;
            set
            {
                if (_isTestMode != value)
                {
                    _isTestMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ClearCrashLogsCommand { get; }
        public ICommand LoadWeeklyCommand { get; }
        public ICommand LoadMonthlyUsageCommand { get; }
        public ICommand ForceWriteNowCommand { get; }

        public ObservableCollection<AppUsageSummary> TodayAppUsage => _todayAppUsage;
        public ObservableCollection<DailySummary> WeeklySummaries => _weeklySummaries;
        public ObservableCollection<MonthlyAppUsageDto> MonthlyAppUsage => _monthlyAppUsage;

        public bool IsMonthlyUsageEmpty
        {
            get => _isMonthlyUsageEmpty;
            private set
            {
                if (_isMonthlyUsageEmpty != value)
                {
                    _isMonthlyUsageEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                var normalized = (value == default ? DateTime.Today : value).Date;
                if (_selectedDate != normalized)
                {
                    _selectedDate = normalized;
                    OnPropertyChanged();
                    ApplyLiveRefreshSettings();
                    RefreshForSelectedDate();

                    if (IsDayDetailsMode)
                    {
                        LoadManualTasksForSelectedDate();
                    }
                }
            }
        }

        public ObservableCollection<ManualTaskEntry> ManualTasks => _manualTasks;

        public ManualTaskEntry? SelectedManualTask
        {
            get => _selectedManualTask;
            set
            {
                if (!ReferenceEquals(_selectedManualTask, value))
                {
                    _selectedManualTask = value;
                    OnPropertyChanged();
                    UpdateManualTaskCommandsCanExecute();

                    if (_selectedManualTask != null)
                    {
                        ManualTaskName = _selectedManualTask.TaskName;
                        var ts = TimeSpan.FromSeconds(_selectedManualTask.TotalSeconds < 0 ? 0 : _selectedManualTask.TotalSeconds);
                        ManualHours = ((int)ts.TotalHours).ToString(CultureInfo.InvariantCulture);
                        ManualMinutes = ts.Minutes.ToString(CultureInfo.InvariantCulture);
                        ManualSeconds = ts.Seconds.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        public string ManualTaskName
        {
            get => _manualTaskName;
            set
            {
                if (!string.Equals(_manualTaskName, value, StringComparison.Ordinal))
                {
                    _manualTaskName = value;
                    OnPropertyChanged();
                    UpdateManualTaskCommandsCanExecute();
                }
            }
        }

        public string ManualHours
        {
            get => _manualHours;
            set
            {
                if (!string.Equals(_manualHours, value, StringComparison.Ordinal))
                {
                    _manualHours = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ManualMinutes
        {
            get => _manualMinutes;
            set
            {
                if (!string.Equals(_manualMinutes, value, StringComparison.Ordinal))
                {
                    _manualMinutes = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ManualSeconds
        {
            get => _manualSeconds;
            set
            {
                if (!string.Equals(_manualSeconds, value, StringComparison.Ordinal))
                {
                    _manualSeconds = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ManualTotalText => FormatTimeSpan(TimeSpan.FromSeconds(_manualTasks.Sum(t => Math.Max(0, t.TotalSeconds))));
        public string GrandTotalText => FormatTimeSpan((TotalActiveTimeToday + TotalIdleTimeToday + TotalLockedTimeToday) + TimeSpan.FromSeconds(_manualTasks.Sum(t => Math.Max(0, t.TotalSeconds))));

        // Segoe MDL2 Assets glyphs
        public string PrimaryManualTaskGlyph => IsManualEditMode ? "\uE74E" : "\uE710"; // Save / Add
        public string ManualRowActionGlyph => IsManualEditMode ? "\uE711" : "\uE70F"; // Cancel / Edit
        public string ManualDeleteGlyph => "\uE74D"; // Delete

        public ICommand PrimaryManualTaskCommand { get; }
        public ICommand BeginEditManualTaskCommand { get; }
        public ICommand CancelManualTaskEditCommand { get; }
        public ICommand DeleteManualTaskRowCommand { get; }

        private void RefreshForSelectedDate()
        {
            if (_trackingService != null && _trackingService.IsRunning && SelectedDate.Date == DateTime.Today)
            {
                _trackingService.FlushCurrentRecord();
            }

            RefreshTodaySummary();
            RefreshWeeklySummary();
            SyncHeaderActiveBaseFromSummary();
        }

        private void UpdateWeekHeaderTexts()
        {
            OnPropertyChanged(nameof(WeekNumberText));
            OnPropertyChanged(nameof(WeekRangeText));
        }

        private void NotifyManualTotalsChanged()
        {
            OnPropertyChanged(nameof(ManualTotalText));
            OnPropertyChanged(nameof(GrandTotalText));
            OnPropertyChanged(nameof(SelectedDayManualTasksText));
            OnPropertyChanged(nameof(SelectedDayTotalActiveText));
            OnPropertyChanged(nameof(MonthlyManualTasksText));
            OnPropertyChanged(nameof(MonthlyTotalActiveText));
        }

        private void ResetManualEditState()
        {
            SelectedManualTask = null;
            IsManualEditMode = false;
        }

        private void NotifySelectedDaySummaryTextsChanged()
        {
            OnPropertyChanged(nameof(SelectedDayActiveTrackedText));
            OnPropertyChanged(nameof(SelectedDayIdleText));
            OnPropertyChanged(nameof(SelectedDayLockedText));
            OnPropertyChanged(nameof(SelectedDayStartText));
            OnPropertyChanged(nameof(SelectedDayEndText));
        }

        private bool IsSelectedDateInCurrentWeek()
        {
            var weekStart = WeekStartDate.Date;
            var weekEnd = weekStart.AddDays(6);
            return SelectedDate.Date >= weekStart && SelectedDate.Date <= weekEnd;
        }

        private bool IsSelectedDateInSelectedMonth()
        {
            return SelectedDate.Year == SelectedMonth.Year && SelectedDate.Month == SelectedMonth.Month;
        }

        private void LoadMonthlyUsage()
        {
            _monthlyAppUsage.Clear();

            var start = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var perProcess = new System.Collections.Generic.Dictionary<string, (TimeSpan Active, TimeSpan Idle, TimeSpan Locked)>(StringComparer.OrdinalIgnoreCase);

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (!_activityLogReader.TryReadDay(date.Date, out var entries))
                {
                    continue;
                }

                foreach (var entry in entries)
                {
                    var duration = entry.EndTime - entry.StartTime;
                    string processName = entry.ProcessName;
                    if (string.IsNullOrWhiteSpace(processName))
                    {
                        processName = "(Unknown)";
                    }

                    if (!perProcess.TryGetValue(processName, out var totals))
                    {
                        totals = (TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
                    }

                    if (entry.IsLocked)
                    {
                        totals.Locked += duration;
                    }
                    else if (entry.IsIdle)
                    {
                        totals.Idle += duration;
                    }
                    else
                    {
                        totals.Active += duration;
                    }

                    perProcess[processName] = totals;
                }
            }

            foreach (var kvp in perProcess
                         .OrderByDescending(k => k.Value.Active)
                         .ThenByDescending(k => k.Value.Idle)
                         .ThenByDescending(k => k.Value.Locked))
            {
                _monthlyAppUsage.Add(new MonthlyAppUsageDto
                {
                    ProcessName = kvp.Key,
                    TotalActive = kvp.Value.Active,
                    TotalIdle = kvp.Value.Idle,
                    TotalLocked = kvp.Value.Locked
                });
            }

            IsMonthlyUsageEmpty = _monthlyAppUsage.Count == 0;

            OnPropertyChanged(nameof(MonthlyActiveTrackedText));
            OnPropertyChanged(nameof(MonthlyManualTasksText));
            OnPropertyChanged(nameof(MonthlyTotalActiveText));
        }

        public string MonthlyActiveTrackedText
        {
            get
            {
                var tracked = TimeSpan.FromSeconds(_monthlyAppUsage.Sum(x => Math.Max(0, x.TotalActive.TotalSeconds)));
                return FormatTimeSpan(tracked);
            }
        }

        public string MonthlyManualTasksText => FormatTimeSpan(TimeSpan.FromSeconds(GetManualSecondsForMonth(SelectedMonth)));

        public string MonthlyTotalActiveText
        {
            get
            {
                var tracked = TimeSpan.FromSeconds(_monthlyAppUsage.Sum(x => Math.Max(0, x.TotalActive.TotalSeconds)));
                var manual = TimeSpan.FromSeconds(GetManualSecondsForMonth(SelectedMonth));
                return FormatTimeSpan(tracked + manual);
            }
        }

        private int GetManualSecondsForMonth(DateTime month)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            int total = 0;
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                foreach (var item in _manualTaskService.Load(date.Date))
                {
                    total += Math.Max(0, item.TotalSeconds);
                }
            }

            return total;
        }

        private int GetManualSecondsForDate(DateTime date)
        {
            try
            {
                return _manualTaskService.Load(date.Date).Sum(x => Math.Max(0, x.TotalSeconds));
            }
            catch
            {
                return 0;
            }
        }

        private void RefreshSelectedDayManualDuration()
        {
            _selectedDayManualDuration = TimeSpan.FromSeconds(GetManualSecondsForDate(SelectedDate.Date));
            OnPropertyChanged(nameof(SelectedDayManualTasksText));
            OnPropertyChanged(nameof(SelectedDayTotalActiveText));
        }

        public DateTime WeekStartDate
        {
            get => _weekStartDate;
            set
            {
                var normalized = value.Date;
                if (_weekStartDate != normalized)
                {
                    _weekStartDate = normalized;
                    OnPropertyChanged();
                    UpdateWeekHeaderTexts();
                }
            }
        }

        public string WeekNumberText => $"Week {ISOWeek.GetWeekOfYear(WeekStartDate.Date)}";

        public string WeekRangeText
        {
            get
            {
                var from = WeekStartDate.Date;
                var to = from.AddDays(6);
                return $"{from:dd-MMM-yyyy} → {to:dd-MMM-yyyy}";
            }
        }

        public int IdleThresholdMinutes
        {
            get => _idleThresholdMinutes;
            set
            {
                if (_idleThresholdMinutes != value)
                {
                    _idleThresholdMinutes = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PollIntervalSeconds
        {
            get => _pollIntervalSeconds;
            set
            {
                if (_pollIntervalSeconds != value)
                {
                    _pollIntervalSeconds = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableLiveRefresh
        {
            get => _enableLiveRefresh;
            set
            {
                if (_enableLiveRefresh != value)
                {
                    _enableLiveRefresh = value;
                    OnPropertyChanged();
                    ApplyLiveRefreshSettings();
                }
            }
        }

        public int LiveRefreshIntervalSeconds
        {
            get => _liveRefreshIntervalSeconds;
            set
            {
                if (_liveRefreshIntervalSeconds != value)
                {
                    _liveRefreshIntervalSeconds = value;
                    OnPropertyChanged();
                    ApplyLiveRefreshSettings();
                }
            }
        }

        public int CrashLogRetentionDays
        {
            get => _crashLogRetentionDays;
            set
            {
                if (_crashLogRetentionDays != value)
                {
                    _crashLogRetentionDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CrashLogMaxSizeMB
        {
            get => _crashLogMaxSizeMB;
            set
            {
                if (_crashLogMaxSizeMB != value)
                {
                    _crashLogMaxSizeMB = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan TotalActiveTimeToday
        {
            get => _totalActiveTimeToday;
            private set
            {
                if (_totalActiveTimeToday != value)
                {
                    _totalActiveTimeToday = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalActiveTimeTodayDisplay));
                    OnPropertyChanged(nameof(SelectedDayActiveTrackedText));
                    OnPropertyChanged(nameof(SelectedDayTotalActiveText));
                    OnPropertyChanged(nameof(GrandTotalText));
                }
            }
        }

        public TimeSpan TotalIdleTimeToday
        {
            get => _totalIdleTimeToday;
            private set
            {
                if (_totalIdleTimeToday != value)
                {
                    _totalIdleTimeToday = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalIdleTimeTodayDisplay));
                    OnPropertyChanged(nameof(SelectedDayIdleText));
                    OnPropertyChanged(nameof(SelectedDayTotalActiveText));
                    OnPropertyChanged(nameof(GrandTotalText));
                }
            }
        }

        public TimeSpan TotalLockedTimeToday
        {
            get => _totalLockedTimeToday;
            private set
            {
                if (_totalLockedTimeToday != value)
                {
                    _totalLockedTimeToday = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalLockedTimeTodayDisplay));
                    OnPropertyChanged(nameof(SelectedDayLockedText));
                    OnPropertyChanged(nameof(SelectedDayTotalActiveText));
                    OnPropertyChanged(nameof(GrandTotalText));
                }
            }
        }

        public string TotalActiveTimeTodayDisplay => FormatTimeSpan(TotalActiveTimeToday);
        public string TotalIdleTimeTodayDisplay => FormatTimeSpan(TotalIdleTimeToday);
        public string TotalLockedTimeTodayDisplay => FormatTimeSpan(TotalLockedTimeToday);

        public string SelectedDayActiveTrackedText => $"{TotalActiveTimeToday.ToHoursMinutes()}";
        public string SelectedDayManualTasksText => $"{_selectedDayManualDuration.ToHoursMinutes()}";
        public string SelectedDayTotalActiveText => $"{(TotalActiveTimeToday + _selectedDayManualDuration).ToHoursMinutes()}";
        public string SelectedDayIdleText => $"{TotalIdleTimeToday.ToHoursMinutes()}";
        public string SelectedDayLockedText => $"{TotalLockedTimeToday.ToHoursMinutes()}";

        public TimeSpan WeeklyTrackedActiveDuration
        {
            get => _weeklyTrackedActiveDuration;
            private set
            {
                if (_weeklyTrackedActiveDuration != value)
                {
                    _weeklyTrackedActiveDuration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeeklyTrackedActiveText));
                }
            }
        }

        public TimeSpan WeeklyManualDuration
        {
            get => _weeklyManualDuration;
            private set
            {
                if (_weeklyManualDuration != value)
                {
                    _weeklyManualDuration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeeklyManualText));
                }
            }
        }

        public string SelectedDayStartText => _selectedDayStartTime.HasValue
            ? $"{_selectedDayStartTime.Value:HH:mm}"
            : "";

        public string SelectedDayEndText => _selectedDayEndTime.HasValue
            ? $"{_selectedDayEndTime.Value:HH:mm}"
            : "";

        public TimeSpan WeeklyTotalActiveDuration
        {
            get => _weeklyTotalActiveDuration;
            private set
            {
                if (_weeklyTotalActiveDuration != value)
                {
                    _weeklyTotalActiveDuration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeeklyTotalActiveText));
                }
            }
        }

        public TimeSpan WeeklyTotalIdleDuration
        {
            get => _weeklyTotalIdleDuration;
            private set
            {
                if (_weeklyTotalIdleDuration != value)
                {
                    _weeklyTotalIdleDuration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeeklyTotalIdleText));
                }
            }
        }

        public TimeSpan WeeklyTotalLockedDuration
        {
            get => _weeklyTotalLockedDuration;
            private set
            {
                if (_weeklyTotalLockedDuration != value)
                {
                    _weeklyTotalLockedDuration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeeklyTotalLockedText));
                }
            }
        }

        public string WeeklyTrackedActiveText => $"{WeeklyTrackedActiveDuration.ToHoursMinutes()}";
        public string WeeklyManualText => $"{WeeklyManualDuration.ToHoursMinutes()}";
        public string WeeklyTotalActiveText => $"{WeeklyTotalActiveDuration.ToHoursMinutes()}";
        public string WeeklyTotalIdleText => $"{WeeklyTotalIdleDuration.ToHoursMinutes()}";
        public string WeeklyTotalLockedText => $"{WeeklyTotalLockedDuration.ToHoursMinutes()}";

        private void SaveSettings()
        {
            _settingsSnapshot.IdleThresholdMinutes = IdleThresholdMinutes;
            _settingsSnapshot.PollIntervalSeconds = PollIntervalSeconds;
            _settingsSnapshot.EnableLiveRefresh = EnableLiveRefresh;
            _settingsSnapshot.LiveRefreshIntervalSeconds = LiveRefreshIntervalSeconds;

            _settingsSnapshot.CrashLogRetentionDays = Clamp(CrashLogRetentionDays, 1, 365);
            _settingsSnapshot.CrashLogMaxSizeMB = Clamp(CrashLogMaxSizeMB, 1, 2048);
            CrashLogRetentionDays = _settingsSnapshot.CrashLogRetentionDays;
            CrashLogMaxSizeMB = _settingsSnapshot.CrashLogMaxSizeMB;

            _settingsService?.Save(_settingsSnapshot);
            _trackingService?.ApplySettings(_settingsSnapshot);
            ApplyLiveRefreshSettings();

            if (System.Windows.Application.Current is App app && app.CloseTrackingService != null)
            {
                app.CloseTrackingService.ApplyCrashLogPolicy(_settingsSnapshot.CrashLogRetentionDays, _settingsSnapshot.CrashLogMaxSizeMB);
                app.CloseTrackingService.CleanupCrashLogs();
            }

            LastCrash.Load();

            RefreshCommand.Execute(null);
        }

        private void ClearCrashLogs()
        {
            if (System.Windows.Application.Current is App app && app.CloseTrackingService != null)
            {
                app.CloseTrackingService.ClearCrashLogs();
            }

            LastCrash.Load();
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private bool CanEditManualTasks()
        {
            return IsDayDetailsMode;
        }

        private void LoadManualTasksForSelectedDate()
        {
            if (!IsDayDetailsMode)
            {
                _manualTasks.Clear();
                ResetManualEditState();
                NotifyManualTotalsChanged();
                return;
            }

            _manualTasks.Clear();
            foreach (var item in _manualTaskService.Load(SelectedDate.Date))
            {
                _manualTasks.Add(item);
            }

            ResetManualEditState();
            NotifyManualTotalsChanged();
        }

        private void PersistManualTasks()
        {
            if (!IsDayDetailsMode)
            {
                return;
            }

            _manualTaskService.Save(SelectedDate.Date, _manualTasks.ToList());

            // Update cached manual duration immediately so Total Active reflects edits without reopening.
            _selectedDayManualDuration = TimeSpan.FromSeconds(_manualTasks.Sum(t => Math.Max(0, t.TotalSeconds)));

            NotifyManualTotalsChanged();

            // Recompute week totals/list if the selected day is within the currently displayed week.
            if (IsSelectedDateInCurrentWeek())
            {
                LoadWeeklySummary();
            }

            // Refresh month rollups if the selected day is within the currently selected month.
            if (IsSelectedDateInSelectedMonth())
            {
                OnPropertyChanged(nameof(MonthlyActiveTrackedText));
                OnPropertyChanged(nameof(MonthlyManualTasksText));
                OnPropertyChanged(nameof(MonthlyTotalActiveText));
            }
        }

        private static int ParseNonNegativeInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return 0;
            }

            return parsed < 0 ? 0 : parsed;
        }

        private static int NormalizeToTotalSeconds(int hours, int minutes, int seconds)
        {
            long total = 0;
            total += (long)Math.Max(0, hours) * 3600L;
            total += (long)Math.Max(0, minutes) * 60L;
            total += (long)Math.Max(0, seconds);
            if (total < 0) total = 0;
            if (total > int.MaxValue) total = int.MaxValue;
            return (int)total;
        }

        private void PrimaryManualTaskAction()
        {
            if (!CanEditManualTasks())
            {
                return;
            }

            if (IsManualEditMode)
            {
                UpdateManualTask();
                return;
            }

            if (string.IsNullOrWhiteSpace(ManualTaskName))
            {
                return;
            }

            int hours = ParseNonNegativeInt(ManualHours);
            int minutes = ParseNonNegativeInt(ManualMinutes);
            int seconds = ParseNonNegativeInt(ManualSeconds);

            int totalSeconds = NormalizeToTotalSeconds(hours, minutes, seconds);

            var entry = new ManualTaskEntry
            {
                TaskName = ManualTaskName.Trim(),
                TotalSeconds = totalSeconds
            };

            _manualTasks.Add(entry);

            ManualTaskName = string.Empty;
            ManualHours = string.Empty;
            ManualMinutes = string.Empty;
            ManualSeconds = string.Empty;

            PersistManualTasks();
            UpdateManualTaskCommandsCanExecute();
        }

        private void BeginEditManualTask(ManualTaskEntry? entry)
        {
            if (!CanEditManualTasks() || entry == null)
            {
                return;
            }

            SelectedManualTask = entry;
            IsManualEditMode = true;
        }

        private void CancelManualTaskEdit()
        {
            IsManualEditMode = false;
            SelectedManualTask = null;
            ManualTaskName = string.Empty;
            ManualHours = string.Empty;
            ManualMinutes = string.Empty;
            ManualSeconds = string.Empty;
        }

        private void UpdateManualTask()
        {
            if (!CanEditManualTasks() || SelectedManualTask == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ManualTaskName))
            {
                return;
            }

            int hours = ParseNonNegativeInt(ManualHours);
            int minutes = ParseNonNegativeInt(ManualMinutes);
            int seconds = ParseNonNegativeInt(ManualSeconds);

            int totalSeconds = NormalizeToTotalSeconds(hours, minutes, seconds);

            SelectedManualTask.TaskName = ManualTaskName.Trim();
            SelectedManualTask.TotalSeconds = totalSeconds;
            PersistManualTasks();
            CancelManualTaskEdit();
        }

        private void DeleteManualTaskRow(ManualTaskEntry? entry)
        {
            if (!CanEditManualTasks() || _isManualEditMode || entry == null)
            {
                return;
            }

            _manualTasks.Remove(entry);
            PersistManualTasks();
            UpdateManualTaskCommandsCanExecute();
        }

        private void UpdateManualTaskCommandsCanExecute()
        {
            if (PrimaryManualTaskCommand is RelayCommand primary)
            {
                primary.RaiseCanExecuteChanged();
            }
            if (BeginEditManualTaskCommand is RelayCommand beginEdit)
            {
                beginEdit.RaiseCanExecuteChanged();
            }
            if (CancelManualTaskEditCommand is RelayCommand cancel)
            {
                cancel.RaiseCanExecuteChanged();
            }
            if (DeleteManualTaskRowCommand is RelayCommand del)
            {
                del.RaiseCanExecuteChanged();
            }
        }

        private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void RefreshTodaySummary()
        {
            TotalActiveTimeToday = TimeSpan.Zero;
            TotalIdleTimeToday = TimeSpan.Zero;
            TotalLockedTimeToday = TimeSpan.Zero;
            _todayAppUsage.Clear();
            _selectedDayStartTime = null;
            _selectedDayEndTime = null;
            _selectedDayManualDuration = TimeSpan.Zero;

            if (!_activityLogReader.TryReadDay(SelectedDate.Date, out var entries))
            {
                RefreshSelectedDayManualDuration();
                NotifySelectedDaySummaryTextsChanged();
                return;
            }

            var perProcessDurations = new System.Collections.Generic.Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                var start = entry.StartTime;
                var end = entry.EndTime;

                if (!_selectedDayStartTime.HasValue || start < _selectedDayStartTime.Value)
                {
                    _selectedDayStartTime = start;
                }

                if (!_selectedDayEndTime.HasValue || end > _selectedDayEndTime.Value)
                {
                    _selectedDayEndTime = end;
                }

                var duration = end - start;

                if (entry.IsLocked)
                {
                    TotalLockedTimeToday += duration;
                }
                else if (entry.IsIdle)
                {
                    TotalIdleTimeToday += duration;
                }
                else
                {
                    TotalActiveTimeToday += duration;

                    string processName = entry.ProcessName;
                    if (!string.IsNullOrEmpty(processName))
                    {
                        if (perProcessDurations.TryGetValue(processName, out var existing))
                        {
                            perProcessDurations[processName] = existing + duration;
                        }
                        else
                        {
                            perProcessDurations[processName] = duration;
                        }
                    }
                }
            }

            foreach (var kvp in perProcessDurations.OrderByDescending(kvp => kvp.Value))
            {
                _todayAppUsage.Add(new AppUsageSummary
                {
                    ProcessName = kvp.Key,
                    ActiveDuration = kvp.Value
                });
            }

            // Ensure labeled summary text updates even if totals did not change
            RefreshSelectedDayManualDuration();
            NotifySelectedDaySummaryTextsChanged();
        }

        private void RefreshSelectedDaySummary()
        {
            RefreshTodaySummary();
        }

        private void RefreshSelectedDayAppUsage()
        {
            RefreshTodaySummary();
        }

        private void RefreshWeeklySummary()
        {
            LoadWeeklySummary();
        }

        private static string FormatTimeSpan(TimeSpan value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", (int)value.TotalHours, value.Minutes);
        }

        public void LoadWeeklySummary()
        {
            _weeklySummaries.Clear();

            UpdateWeekHeaderTexts();

            for (int i = 0; i < 7; i++)
            {
                DateTime date = WeekStartDate.Date.AddDays(i);

                TimeSpan active = TimeSpan.Zero;
                TimeSpan idle = TimeSpan.Zero;
                TimeSpan locked = TimeSpan.Zero;
                TimeSpan manual = TimeSpan.FromSeconds(GetManualSecondsForDate(date.Date));

                if (_activityLogReader.TryReadDay(date.Date, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        var duration = entry.EndTime - entry.StartTime;
                        if (entry.IsLocked)
                        {
                            locked += duration;
                        }
                        else if (entry.IsIdle)
                        {
                            idle += duration;
                        }
                        else
                        {
                            active += duration;
                        }
                    }
                }

                _weeklySummaries.Add(new DailySummary
                {
                    Date = date,
                    ActiveDuration = active,
                    ManualTaskDuration = manual,
                    IdleDuration = idle,
                    LockedDuration = locked
                });
            }

            WeeklyTrackedActiveDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.ActiveDuration.Ticks));
            WeeklyManualDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.ManualTaskDuration.Ticks));
            WeeklyTotalActiveDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.TotalActiveDuration.Ticks));
            WeeklyTotalIdleDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.IdleDuration.Ticks));
            WeeklyTotalLockedDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.LockedDuration.Ticks));
        }

        private void ApplyLiveRefreshSettings()
        {
            if (_autoRefreshTimer == null)
            {
                return;
            }

            int interval = LiveRefreshIntervalSeconds;
            if (interval <= 0)
            {
                interval = 30;
                _liveRefreshIntervalSeconds = interval;
                OnPropertyChanged(nameof(LiveRefreshIntervalSeconds));
            }

            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(interval);

            if (!EnableLiveRefresh || _trackingService == null || !_trackingService.IsRunning || SelectedDate.Date != DateTime.Today)
            {
                _autoRefreshTimer.Stop();
                return;
            }

            _autoRefreshTimer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class RelayCommand : ICommand
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

            public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

            public event EventHandler? CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
    }
}
