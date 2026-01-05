using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using SystemActivityTracker.Models;
using SystemActivityTracker.Services;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly TrackingService? _trackingService;
        private readonly SettingsService? _settingsService;
        private readonly ManualTaskService _manualTaskService = new ManualTaskService();
        private string _trackingStatus = "Tracking status: Stopped";
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
        private DateTime _selectedDate = DateTime.Today;
        private DateTime _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _weekStartDate;
        private readonly ObservableCollection<DailySummary> _weeklySummaries = new ObservableCollection<DailySummary>();
        private readonly ObservableCollection<MonthlyAppUsageDto> _monthlyAppUsage = new ObservableCollection<MonthlyAppUsageDto>();
        private bool _isMonthlyUsageEmpty = true;
        private TimeSpan _weeklyTotalActiveDuration;
        private TimeSpan _weeklyTotalIdleDuration;
        private TimeSpan _weeklyTotalLockedDuration;
        private DateTime? _selectedDayStartTime;
        private DateTime? _selectedDayEndTime;
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

        public MainWindowViewModel(TrackingService? trackingService, SettingsService? settingsService = null)
        {
            _trackingService = trackingService;
            _settingsService = settingsService;
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
            if (settings.IdleThresholdMinutes <= 0) settings.IdleThresholdMinutes = 2;
            if (settings.PollIntervalSeconds <= 0) settings.PollIntervalSeconds = 5;
            if (settings.LiveRefreshIntervalSeconds <= 0) settings.LiveRefreshIntervalSeconds = 30;

            _settingsSnapshot = settings;

            IdleThresholdMinutes = settings.IdleThresholdMinutes;
            PollIntervalSeconds = settings.PollIntervalSeconds;
            EnableLiveRefresh = settings.EnableLiveRefresh;
            LiveRefreshIntervalSeconds = settings.LiveRefreshIntervalSeconds;

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
            TrackingStatus = "Tracking status: Running";
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
                TrackingStatus = "Tracking status: Stopped";
                StopRunningTimerTicker();
                _autoRefreshTimer.Stop();
                return;
            }

            _trackingService.Stop();
            TrackingStatus = "Tracking status: Stopped";
            StopRunningTimerTicker();
            ApplyLiveRefreshSettings();
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

            bool isRunning = false;
            DateTime? currentRecordStart = null;
            bool isLocked = false;
            bool isIdle = false;

            bool hasSnapshot = _trackingService != null && _trackingService.TryGetCurrentStateSnapshot(
                out isRunning,
                out currentRecordStart,
                out isLocked,
                out isIdle);

            bool isActive = hasSnapshot && !isLocked && !isIdle;

            if (isActive)
            {
                if (_headerActiveStartLocal == null)
                {
                    _headerActiveStartLocal = currentRecordStart ?? now;
                    _headerActiveLastRecordStartLocal = currentRecordStart;
                }
                else if (currentRecordStart.HasValue && _headerActiveLastRecordStartLocal.HasValue && currentRecordStart.Value != _headerActiveLastRecordStartLocal.Value)
                {
                    _headerActiveBase += now - _headerActiveStartLocal.Value;
                    _headerActiveStartLocal = currentRecordStart.Value;
                    _headerActiveLastRecordStartLocal = currentRecordStart.Value;
                }
            }
            else
            {
                if (_headerActiveStartLocal.HasValue)
                {
                    _headerActiveBase += now - _headerActiveStartLocal.Value;
                    _headerActiveStartLocal = null;
                    _headerActiveLastRecordStartLocal = null;
                }
            }

            var total = _headerActiveBase;
            if (_headerActiveStartLocal.HasValue)
            {
                total += now - _headerActiveStartLocal.Value;
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
            HeaderRunningTimerText = "Active - " + string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        private void SyncHeaderActiveBaseFromSummary()
        {
            _headerActiveBase = ComputeActiveTotalForDate(DateTime.Today);
            _headerActiveStartLocal = null;
            _headerActiveLastRecordStartLocal = null;
            _lastDisplayedActiveSecond = -1;
            TickHeaderActiveTimer();
        }

        private static TimeSpan ComputeActiveTotalForDate(DateTime date)
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, "SystemActivityTracker");
            string fileName = $"activity-log-{date:yyyy-MM-dd}.csv";
            string filePath = Path.Combine(appFolder, fileName);

            if (!File.Exists(filePath))
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalActive = TimeSpan.Zero;

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("StartTime", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fields = ParseCsvLine(line);
                if (fields.Length < 6)
                {
                    continue;
                }

                if (!DateTime.TryParse(fields[0], null, DateTimeStyles.RoundtripKind, out var start))
                {
                    continue;
                }

                if (!DateTime.TryParse(fields[1], null, DateTimeStyles.RoundtripKind, out var end))
                {
                    continue;
                }

                if (!bool.TryParse(fields[4], out var isLocked))
                {
                    continue;
                }

                if (!bool.TryParse(fields[5], out var isIdle))
                {
                    continue;
                }

                if (end < start)
                {
                    continue;
                }

                if (!isLocked && !isIdle)
                {
                    totalActive += end - start;
                }
            }

            return totalActive;
        }

        private void TickHeaderActiveTimer()
        {
            DateTime now = DateTime.Now;

            bool isRunning = false;
            DateTime? currentRecordStart = null;
            bool isLocked = false;
            bool isIdle = false;

            bool hasSnapshot = _trackingService != null && _trackingService.TryGetCurrentStateSnapshot(
                out isRunning,
                out currentRecordStart,
                out isLocked,
                out isIdle);

            bool isActive = hasSnapshot && !isLocked && !isIdle;

            if (isActive)
            {
                if (_headerActiveStartLocal == null)
                {
                    _headerActiveStartLocal = currentRecordStart ?? now;
                    _headerActiveLastRecordStartLocal = currentRecordStart;
                }
                else if (currentRecordStart.HasValue && _headerActiveLastRecordStartLocal.HasValue && currentRecordStart.Value != _headerActiveLastRecordStartLocal.Value)
                {
                    _headerActiveBase += now - _headerActiveStartLocal.Value;
                    _headerActiveStartLocal = currentRecordStart.Value;
                    _headerActiveLastRecordStartLocal = currentRecordStart.Value;
                }
            }
            else
            {
                if (_headerActiveStartLocal.HasValue)
                {
                    _headerActiveBase += now - _headerActiveStartLocal.Value;
                    _headerActiveStartLocal = null;
                    _headerActiveLastRecordStartLocal = null;
                }
            }

            var total = _headerActiveBase;
            if (_headerActiveStartLocal.HasValue)
            {
                total += now - _headerActiveStartLocal.Value;
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
            HeaderActiveTimerText = "Total active - " + string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
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
                }
            }
        }

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

        private void LoadMonthlyUsage()
        {
            _monthlyAppUsage.Clear();

            var start = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, "SystemActivityTracker");

            var perProcess = new System.Collections.Generic.Dictionary<string, (TimeSpan Active, TimeSpan Idle, TimeSpan Locked)>(StringComparer.OrdinalIgnoreCase);

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                string fileName = $"activity-log-{date:yyyy-MM-dd}.csv";
                string filePath = Path.Combine(appFolder, fileName);

                if (!File.Exists(filePath))
                {
                    continue;
                }

                foreach (var line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("StartTime", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var fields = ParseCsvLine(line);
                    if (fields.Length < 6)
                    {
                        continue;
                    }

                    if (!DateTime.TryParse(fields[0], null, DateTimeStyles.RoundtripKind, out var startTime))
                    {
                        continue;
                    }

                    if (!DateTime.TryParse(fields[1], null, DateTimeStyles.RoundtripKind, out var endTime))
                    {
                        continue;
                    }

                    if (!bool.TryParse(fields[4], out var isLocked))
                    {
                        continue;
                    }

                    if (!bool.TryParse(fields[5], out var isIdle))
                    {
                        continue;
                    }

                    if (endTime < startTime)
                    {
                        continue;
                    }

                    var duration = endTime - startTime;
                    string processName = fields.Length > 2 ? fields[2] : string.Empty;
                    if (string.IsNullOrWhiteSpace(processName))
                    {
                        processName = "(Unknown)";
                    }

                    if (!perProcess.TryGetValue(processName, out var totals))
                    {
                        totals = (TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
                    }

                    if (isLocked)
                    {
                        totals.Locked += duration;
                    }
                    else if (isIdle)
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

            OnPropertyChanged(nameof(MonthlyTotalText));
        }

        public string MonthlyTotalText
        {
            get
            {
                var tracked = TimeSpan.FromSeconds(_monthlyAppUsage.Sum(x => Math.Max(0, x.TotalActive.TotalSeconds + x.TotalIdle.TotalSeconds + x.TotalLocked.TotalSeconds)));
                var manual = GetManualSecondsForMonth(SelectedMonth);
                return FormatTimeSpan(tracked + TimeSpan.FromSeconds(manual));
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
                    OnPropertyChanged(nameof(SelectedDayActiveText));
                    OnPropertyChanged(nameof(SelectedDayTotalText));
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
                    OnPropertyChanged(nameof(SelectedDayTotalText));
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
                    OnPropertyChanged(nameof(SelectedDayTotalText));
                    OnPropertyChanged(nameof(GrandTotalText));
                }
            }
        }

        public string TotalActiveTimeTodayDisplay => FormatTimeSpan(TotalActiveTimeToday);
        public string TotalIdleTimeTodayDisplay => FormatTimeSpan(TotalIdleTimeToday);
        public string TotalLockedTimeTodayDisplay => FormatTimeSpan(TotalLockedTimeToday);

        public string SelectedDayActiveText => $"{TotalActiveTimeToday.ToHoursMinutes()}";
        public string SelectedDayIdleText => $"{TotalIdleTimeToday.ToHoursMinutes()}";
        public string SelectedDayLockedText => $"{TotalLockedTimeToday.ToHoursMinutes()}";

        public string SelectedDayTotalText
        {
            get
            {
                var manual = TimeSpan.FromSeconds(_manualTasks.Sum(t => Math.Max(0, t.TotalSeconds)));
                var tracked = TotalActiveTimeToday + TotalIdleTimeToday + TotalLockedTimeToday;
                return FormatTimeSpan(tracked + manual);
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

        public string WeeklyTotalActiveText => $"{WeeklyTotalActiveDuration.ToHoursMinutes()}";
        public string WeeklyTotalIdleText => $"{WeeklyTotalIdleDuration.ToHoursMinutes()}";
        public string WeeklyTotalLockedText => $"{WeeklyTotalLockedDuration.ToHoursMinutes()}";

        private void SaveSettings()
        {
            _settingsSnapshot.IdleThresholdMinutes = IdleThresholdMinutes;
            _settingsSnapshot.PollIntervalSeconds = PollIntervalSeconds;
            _settingsSnapshot.EnableLiveRefresh = EnableLiveRefresh;
            _settingsSnapshot.LiveRefreshIntervalSeconds = LiveRefreshIntervalSeconds;

            _settingsService?.Save(_settingsSnapshot);
            _trackingService?.ApplySettings(_settingsSnapshot);
            ApplyLiveRefreshSettings();

            RefreshCommand.Execute(null);
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
                SelectedManualTask = null;
                IsManualEditMode = false;
                OnPropertyChanged(nameof(ManualTotalText));
                OnPropertyChanged(nameof(GrandTotalText));
                OnPropertyChanged(nameof(SelectedDayTotalText));
                OnPropertyChanged(nameof(MonthlyTotalText));
                return;
            }

            _manualTasks.Clear();
            foreach (var item in _manualTaskService.Load(SelectedDate.Date))
            {
                _manualTasks.Add(item);
            }

            SelectedManualTask = null;
            IsManualEditMode = false;
            OnPropertyChanged(nameof(ManualTotalText));
            OnPropertyChanged(nameof(GrandTotalText));
            OnPropertyChanged(nameof(SelectedDayTotalText));
            OnPropertyChanged(nameof(MonthlyTotalText));
        }

        private void PersistManualTasks()
        {
            if (!IsDayDetailsMode)
            {
                return;
            }

            _manualTaskService.Save(SelectedDate.Date, _manualTasks.ToList());
            OnPropertyChanged(nameof(ManualTotalText));
            OnPropertyChanged(nameof(GrandTotalText));
            OnPropertyChanged(nameof(SelectedDayTotalText));
            OnPropertyChanged(nameof(MonthlyTotalText));
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

            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, "SystemActivityTracker");
            string fileName = $"activity-log-{SelectedDate:yyyy-MM-dd}.csv";
            string filePath = Path.Combine(appFolder, fileName);

            if (!File.Exists(filePath))
            {
                OnPropertyChanged(nameof(SelectedDayActiveText));
                OnPropertyChanged(nameof(SelectedDayIdleText));
                OnPropertyChanged(nameof(SelectedDayLockedText));
                OnPropertyChanged(nameof(SelectedDayStartText));
                OnPropertyChanged(nameof(SelectedDayEndText));
                return;
            }

            var perProcessDurations = new System.Collections.Generic.Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("StartTime", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // header
                }

                var fields = ParseCsvLine(line);
                if (fields.Length < 6)
                {
                    continue;
                }

                if (!DateTime.TryParse(fields[0], null, DateTimeStyles.RoundtripKind, out var start))
                {
                    continue;
                }

                if (!DateTime.TryParse(fields[1], null, DateTimeStyles.RoundtripKind, out var end))
                {
                    continue;
                }

                if (!_selectedDayStartTime.HasValue || start < _selectedDayStartTime.Value)
                {
                    _selectedDayStartTime = start;
                }

                if (!_selectedDayEndTime.HasValue || end > _selectedDayEndTime.Value)
                {
                    _selectedDayEndTime = end;
                }

                if (!bool.TryParse(fields[4], out var isLocked))
                {
                    continue;
                }

                if (!bool.TryParse(fields[5], out var isIdle))
                {
                    continue;
                }

                if (end < start)
                {
                    continue;
                }

                var duration = end - start;

                if (isLocked)
                {
                    TotalLockedTimeToday += duration;
                }
                else if (isIdle)
                {
                    TotalIdleTimeToday += duration;
                }
                else
                {
                    TotalActiveTimeToday += duration;

                    string processName = fields.Length > 2 ? fields[2] : string.Empty;
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
            OnPropertyChanged(nameof(SelectedDayActiveText));
            OnPropertyChanged(nameof(SelectedDayIdleText));
            OnPropertyChanged(nameof(SelectedDayLockedText));
            OnPropertyChanged(nameof(SelectedDayStartText));
            OnPropertyChanged(nameof(SelectedDayEndText));
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

        private static string[] ParseCsvLine(string line)
        {
            var result = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        public void LoadWeeklySummary()
        {
            _weeklySummaries.Clear();

            UpdateWeekHeaderTexts();

            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(baseFolder, "SystemActivityTracker");

            for (int i = 0; i < 7; i++)
            {
                DateTime date = WeekStartDate.Date.AddDays(i);
                string fileName = $"activity-log-{date:yyyy-MM-dd}.csv";
                string filePath = Path.Combine(appFolder, fileName);

                TimeSpan active = TimeSpan.Zero;
                TimeSpan idle = TimeSpan.Zero;
                TimeSpan locked = TimeSpan.Zero;

                if (File.Exists(filePath))
                {
                    foreach (var line in File.ReadLines(filePath))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (line.StartsWith("StartTime", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var fields = ParseCsvLine(line);
                        if (fields.Length < 6)
                        {
                            continue;
                        }

                        if (!DateTime.TryParse(fields[0], null, DateTimeStyles.RoundtripKind, out var start))
                        {
                            continue;
                        }

                        if (!DateTime.TryParse(fields[1], null, DateTimeStyles.RoundtripKind, out var end))
                        {
                            continue;
                        }

                        if (!bool.TryParse(fields[4], out var isLocked))
                        {
                            continue;
                        }

                        if (!bool.TryParse(fields[5], out var isIdle))
                        {
                            continue;
                        }

                        if (end < start)
                        {
                            continue;
                        }

                        var duration = end - start;

                        if (isLocked)
                        {
                            locked += duration;
                        }
                        else if (isIdle)
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
                    IdleDuration = idle,
                    LockedDuration = locked
                });
            }

            WeeklyTotalActiveDuration = TimeSpan.FromTicks(_weeklySummaries.Sum(d => d.ActiveDuration.Ticks));
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
