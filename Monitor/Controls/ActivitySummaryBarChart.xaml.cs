using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using GradientStop = System.Windows.Media.GradientStop;
using LinearGradientBrush = System.Windows.Media.LinearGradientBrush;

namespace SystemActivityTracker.Controls
{
    public class ActivityDataPoint
    {
        public string Category { get; set; }
        public double Hours { get; set; }
        public Brush Color { get; set; }
    }

    public partial class ActivitySummaryBarChart : UserControl, INotifyPropertyChanged
    {
        #region DependencyProperties

        public static readonly DependencyProperty ChartTitleProperty =
            DependencyProperty.Register(nameof(ChartTitle), typeof(string), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TotalActiveSecondsProperty =
            DependencyProperty.Register(nameof(TotalActiveSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(0L, OnDataChanged));

        public static readonly DependencyProperty LockedSecondsProperty =
            DependencyProperty.Register(nameof(LockedSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(0L, OnDataChanged));

        public static readonly DependencyProperty IdleSecondsProperty =
            DependencyProperty.Register(nameof(IdleSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(0L, OnDataChanged));

        public static readonly DependencyProperty ActiveSecondsProperty =
            DependencyProperty.Register(nameof(ActiveSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(0L, OnDataChanged));

        public static readonly DependencyProperty ManualSecondsProperty =
            DependencyProperty.Register(nameof(ManualSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(0L, OnDataChanged));

        public static readonly DependencyProperty ShowTargetLineProperty =
            DependencyProperty.Register(nameof(ShowTargetLine), typeof(bool), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(true, OnTargetLineChanged));

        public static readonly DependencyProperty TargetSecondsProperty =
            DependencyProperty.Register(nameof(TargetSeconds), typeof(long), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(28800L, OnTargetLineChanged)); // 8 hours default

        public static readonly DependencyProperty EnableTotalActiveThresholdColorsProperty =
            DependencyProperty.Register(nameof(EnableTotalActiveThresholdColors), typeof(bool), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(true, OnDataChanged));

        public static readonly DependencyProperty DebugUseSampleDataProperty =
            DependencyProperty.Register(nameof(DebugUseSampleData), typeof(bool), typeof(ActivitySummaryBarChart),
                new PropertyMetadata(false, OnDataChanged));

        #endregion

        #region Properties

        public string ChartTitle
        {
            get => (string)GetValue(ChartTitleProperty);
            set => SetValue(ChartTitleProperty, value);
        }

        public long TotalActiveSeconds
        {
            get => (long)GetValue(TotalActiveSecondsProperty);
            set => SetValue(TotalActiveSecondsProperty, value);
        }

        public long LockedSeconds
        {
            get => (long)GetValue(LockedSecondsProperty);
            set => SetValue(LockedSecondsProperty, value);
        }

        public long IdleSeconds
        {
            get => (long)GetValue(IdleSecondsProperty);
            set => SetValue(IdleSecondsProperty, value);
        }

        public long ActiveSeconds
        {
            get => (long)GetValue(ActiveSecondsProperty);
            set => SetValue(ActiveSecondsProperty, value);
        }

        public long ManualSeconds
        {
            get => (long)GetValue(ManualSecondsProperty);
            set => SetValue(ManualSecondsProperty, value);
        }

        public bool ShowTargetLine
        {
            get => (bool)GetValue(ShowTargetLineProperty);
            set => SetValue(ShowTargetLineProperty, value);
        }

        public long TargetSeconds
        {
            get => (long)GetValue(TargetSecondsProperty);
            set => SetValue(TargetSecondsProperty, value);
        }

        public bool EnableTotalActiveThresholdColors
        {
            get => (bool)GetValue(EnableTotalActiveThresholdColorsProperty);
            set => SetValue(EnableTotalActiveThresholdColorsProperty, value);
        }

        public bool DebugUseSampleData
        {
            get => (bool)GetValue(DebugUseSampleDataProperty);
            set => SetValue(DebugUseSampleDataProperty, value);
        }

        #endregion

        #region ViewModel Properties

        public SeriesCollection ChartSeries { get; private set; }
        public string[] CategoryLabels { get; private set; } = { "Total Active", "Locked", "Idle" };
        public Func<double, string> AxisLabelFormatter { get; private set; }
        public double YAxisMax { get; private set; }
        public double TargetLinePosition { get; private set; }
        public string TargetLabel { get; private set; }

        // Persistent data collection to prevent chart from resetting
        private ObservableCollection<ActivityDataPoint> _chartDataPoints = new ObservableCollection<ActivityDataPoint>();
        public ObservableCollection<ActivityDataPoint> ChartDataPoints => _chartDataPoints;

        // Tooltip properties
        public string TooltipTotalActive { get; private set; }
        public string TooltipActive { get; private set; }
        public string TooltipManual { get; private set; }
        public string TooltipIdle { get; private set; }
        public string TooltipLocked { get; private set; }

        // Fixed color properties
        public Brush LockedBarColor { get; private set; } = new SolidColorBrush(Color.FromRgb(220, 20, 60)); // Crimson Red
        public Brush IdleBarColor { get; private set; } = new SolidColorBrush(Color.FromRgb(20, 184, 166)); // Teal

        #endregion

        public ActivitySummaryBarChart()
        {
            InitializeComponent();
            this.Loaded += ActivitySummaryBarChart_Loaded;
        }

        private void ActivitySummaryBarChart_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize persistent data collection
            InitializePersistentData();
            // Initial rebuild
            RebuildChartData();
        }

        private void InitializePersistentData()
        {
            // Ensure collection has exactly 3 points (initialize if empty)
            if (_chartDataPoints.Count == 0)
            {
                _chartDataPoints.Add(new ActivityDataPoint { Category = "Total Active", Hours = 0, Color = new SolidColorBrush(Color.FromRgb(37, 99, 235)) });
                _chartDataPoints.Add(new ActivityDataPoint { Category = "Locked", Hours = 0, Color = LockedBarColor });
                _chartDataPoints.Add(new ActivityDataPoint { Category = "Idle", Hours = 0, Color = IdleBarColor });
            }
            
            // Initialize chart series using the persistent collection
            ChartSeries = new SeriesCollection();
            
            var totalActiveSeries = new ColumnSeries
            {
                Title = "Total Active",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0
            };
            
            var lockedSeries = new ColumnSeries
            {
                Title = "Locked",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0,
                Fill = LockedBarColor
            };
            
            var idleSeries = new ColumnSeries
            {
                Title = "Idle",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0,
                Fill = IdleBarColor
            };
            
            ChartSeries.Add(totalActiveSeries);
            ChartSeries.Add(lockedSeries);
            ChartSeries.Add(idleSeries);
        }

        private void RebuildChartData()
        {
            // Ensure we're on UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(RebuildChartData);
                return;
            }

            // Get data values
            long totalActiveSeconds, lockedSeconds, idleSeconds, activeSeconds, manualSeconds;
            if (DebugUseSampleData)
            {
                totalActiveSeconds = 22098; // 6.14 hours
                lockedSeconds = 6505;       // 1.81 hours
                idleSeconds = 87;           // 0.02 hours
                activeSeconds = 14400;      // 4 hours
                manualSeconds = 7698;       // 2.14 hours
            }
            else
            {
                totalActiveSeconds = TotalActiveSeconds;
                lockedSeconds = LockedSeconds;
                idleSeconds = IdleSeconds;
                activeSeconds = ActiveSeconds;
                manualSeconds = ManualSeconds;
            }

            // Convert seconds to hours for plotting
            var totalActiveHours = (double)totalActiveSeconds / 3600.0;
            var lockedHours = (double)lockedSeconds / 3600.0;
            var idleHours = (double)idleSeconds / 3600.0;

            // Update persistent collection (never null/empty)
            _chartDataPoints[0].Hours = totalActiveHours;
            _chartDataPoints[1].Hours = lockedHours;
            _chartDataPoints[2].Hours = idleHours;

            // Update chart series values
            if (ChartSeries.Count >= 3)
            {
                ChartSeries[0].Values[0] = totalActiveHours;
                ChartSeries[1].Values[0] = lockedHours;
                ChartSeries[2].Values[0] = idleHours;

                // Apply conditional coloring to Total Active
                if (ChartSeries[0] is ColumnSeries totalActiveSeries)
                {
                    totalActiveSeries.Fill = EnableTotalActiveThresholdColors ? 
                        GetTotalActiveColor(totalActiveSeconds / 60.0) : 
                        new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    totalActiveSeries.LabelPoint = point => FormatSummaryLabel(totalActiveSeconds);
                }

                // Update label formatters
                if (ChartSeries[1] is ColumnSeries lockedSeries)
                {
                    lockedSeries.LabelPoint = point => FormatSummaryLabel(lockedSeconds);
                }

                if (ChartSeries[2] is ColumnSeries idleSeries)
                {
                    idleSeries.LabelPoint = point => FormatSummaryLabel(idleSeconds);
                }
            }

            // Calculate axis max in hours
            var targetHours = (double)TargetSeconds / 3600.0;
            var maxHours = Math.Max(totalActiveHours, Math.Max(lockedHours, idleHours));
            YAxisMax = Math.Max(targetHours, maxHours) * 1.1; // 10% padding

            // Update target line position
            UpdateTargetLine();

            // Update tooltip data
            TooltipTotalActive = $"Total Active: {FormatDetailedTooltip(totalActiveSeconds)}";
            TooltipActive = $"Active: {FormatDetailedTooltip(activeSeconds)}";
            TooltipManual = $"Manual: {FormatDetailedTooltip(manualSeconds)}";
            TooltipIdle = $"Idle: {FormatDetailedTooltip(idleSeconds)}";
            TooltipLocked = $"Locked: {FormatDetailedTooltip(lockedSeconds)}";

            // Notify property changes
            OnPropertyChanged(nameof(YAxisMax));
            OnPropertyChanged(nameof(TooltipTotalActive));
            OnPropertyChanged(nameof(TooltipActive));
            OnPropertyChanged(nameof(TooltipManual));
            OnPropertyChanged(nameof(TooltipIdle));
            OnPropertyChanged(nameof(TooltipLocked));
        }

        private void TestHardcodedSeries()
        {
            // Create test series with hardcoded values (convert seconds to hours)
            var testSeries = new SeriesCollection();
            
            var totalActiveSeries = new ColumnSeries
            {
                Title = "Total Active",
                Values = new ChartValues<double> { 6.14 }, // 6.14 hours (22098 seconds / 3600)
                DataLabels = true,
                StrokeThickness = 0,
                Fill = new SolidColorBrush(Color.FromRgb(0, 100, 0)) // Green
            };
            
            var lockedSeries = new ColumnSeries
            {
                Title = "Locked", 
                Values = new ChartValues<double> { 2.0 }, // 2 hours (7200 seconds / 3600)
                DataLabels = true,
                StrokeThickness = 0,
                Fill = LockedBarColor
            };
            
            var idleSeries = new ColumnSeries
            {
                Title = "Idle",
                Values = new ChartValues<double> { 1.0 }, // 1 hour (3600 seconds / 3600)
                DataLabels = true,
                StrokeThickness = 0,
                Fill = IdleBarColor
            };
            
            testSeries.Add(totalActiveSeries);
            testSeries.Add(lockedSeries);
            testSeries.Add(idleSeries);
            
            // Assign to chart
            ActivityChart.Series = testSeries;
            
            // Set axis max to include all values with padding (in hours)
            if (ActivityChart.AxisY[0] is Axis yAxis)
            {
                yAxis.MaxValue = 8.0; // 8 hours max
            }
            
            // Debug output
            System.Diagnostics.Debug.WriteLine($"DEBUG: Test series created with {testSeries.Count} series");
            System.Diagnostics.Debug.WriteLine($"DEBUG: TotalActive value: {testSeries[0].Values[0]} hours");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Locked value: {testSeries[1].Values[0]} hours");
            System.Diagnostics.Debug.WriteLine($"DEBUG: Idle value: {testSeries[2].Values[0]} hours");
        }

        private void InitializeChart()
        {
            ChartSeries = new SeriesCollection();
            
            // Total Active series (index 0)
            var totalActiveSeries = new ColumnSeries
            {
                Title = "Total Active",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0
            };
            
            // Locked series (index 1)
            var lockedSeries = new ColumnSeries
            {
                Title = "Locked",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0,
                Fill = LockedBarColor
            };
            
            // Idle series (index 2)
            var idleSeries = new ColumnSeries
            {
                Title = "Idle",
                Values = new ChartValues<double> { 0.0 },
                DataLabels = true,
                StrokeThickness = 0,
                Fill = IdleBarColor
            };
            
            ChartSeries.Add(totalActiveSeries);
            ChartSeries.Add(lockedSeries);
            ChartSeries.Add(idleSeries);

            // Initialize axis formatter
            AxisLabelFormatter = value =>
            {
                var seconds = value;
                if (Math.Abs(seconds - TargetSeconds) < 30) // Target seconds with small tolerance
                {
                    var hours = TargetSeconds / 3600;
                    return $"{hours}h";
                }
                return ""; // Hide all other labels
            };
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActivitySummaryBarChart chart)
            {
                chart.RebuildChartData();
            }
        }

        private static void OnTargetLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ActivitySummaryBarChart chart)
            {
                chart.RebuildChartData();
            }
        }

        private void UpdateData()
        {
            if (ChartSeries == null || ChartSeries.Count < 3) return;

            // Use sample data in debug mode
            long totalActiveSeconds, lockedSeconds, idleSeconds, activeSeconds, manualSeconds;
            if (DebugUseSampleData)
            {
                totalActiveSeconds = 22098; // 6.14 hours
                lockedSeconds = 6505;       // 1.81 hours
                idleSeconds = 87;           // 0.02 hours
                activeSeconds = 14400;      // 4 hours
                manualSeconds = 7698;       // 2.14 hours
            }
            else
            {
                totalActiveSeconds = TotalActiveSeconds;
                lockedSeconds = LockedSeconds;
                idleSeconds = IdleSeconds;
                activeSeconds = ActiveSeconds;
                manualSeconds = ManualSeconds;
            }

            // Convert seconds to hours for chart plotting
            var totalActiveHours = (double)totalActiveSeconds / 3600.0;
            var lockedHours = (double)lockedSeconds / 3600.0;
            var idleHours = (double)idleSeconds / 3600.0;
            
            // Calculate Y-axis max in hours
            var targetHours = (double)TargetSeconds / 3600.0;
            var maxHours = Math.Max(totalActiveHours, Math.Max(lockedHours, idleHours));
            YAxisMax = Math.Max(targetHours, maxHours) * 1.1; // 10% padding
            
            // Update chart series values
            if (ChartSeries[0].Values.Count > 0)
            {
                ChartSeries[0].Values[0] = totalActiveHours;
                if (ChartSeries[0] is ColumnSeries totalActiveSeries)
                {
                    totalActiveSeries.Fill = EnableTotalActiveThresholdColors ? 
                        GetTotalActiveColor(totalActiveSeconds / 60.0) : 
                        new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    totalActiveSeries.LabelPoint = point => FormatSummaryLabel(totalActiveSeconds);
                }
            }

            // Update Locked series (index 1)
            if (ChartSeries[1].Values.Count > 0)
            {
                ChartSeries[1].Values[0] = lockedHours;
                // Summary label on bar
                if (ChartSeries[1] is ColumnSeries lockedSeries)
                {
                    lockedSeries.LabelPoint = point => FormatSummaryLabel(lockedSeconds);
                }
            }

            // Update Idle series (index 2)
            if (ChartSeries[2].Values.Count > 0)
            {
                ChartSeries[2].Values[0] = idleHours;
                // Summary label on bar
                if (ChartSeries[2] is ColumnSeries idleSeries)
                {
                    idleSeries.LabelPoint = point => FormatSummaryLabel(idleSeconds);
                }
            }

            // Update tooltip data
            TooltipTotalActive = $"Total Active: {FormatDetailedTooltip(totalActiveSeconds)}";
            TooltipActive = $"Active: {FormatDetailedTooltip(activeSeconds)}";
            TooltipManual = $"Manual: {FormatDetailedTooltip(manualSeconds)}";
            TooltipIdle = $"Idle: {FormatDetailedTooltip(idleSeconds)}";
            TooltipLocked = $"Locked: {FormatDetailedTooltip(lockedSeconds)}";

            // Update target line
            UpdateTargetLine();

            // Notify property changes
            OnPropertyChanged(nameof(YAxisMax));
            OnPropertyChanged(nameof(TooltipTotalActive));
            OnPropertyChanged(nameof(TooltipActive));
            OnPropertyChanged(nameof(TooltipManual));
            OnPropertyChanged(nameof(TooltipIdle));
            OnPropertyChanged(nameof(TooltipLocked));
        }

        private void UpdateTargetLine()
        {
            var chartHeight = 180.0;
            var targetHours = (double)TargetSeconds / 3600.0;
            TargetLinePosition = chartHeight - (targetHours / YAxisMax * chartHeight);
            
            var hours = TargetSeconds / 3600;
            TargetLabel = $"{hours}h";

            OnPropertyChanged(nameof(TargetLinePosition));
            OnPropertyChanged(nameof(TargetLabel));
        }

        private Brush GetTotalActiveColor(double totalActiveMinutes)
        {
            var totalActiveHours = totalActiveMinutes / 60.0;
            
            if (totalActiveHours < 4)
            {
                // LowDarkRed
                return new SolidColorBrush(Color.FromRgb(139, 0, 0));
            }
            else if (totalActiveHours < 6)
            {
                // MediumAmber
                return new SolidColorBrush(Color.FromRgb(255, 191, 0));
            }
            else if (totalActiveHours < 8)
            {
                // GoingToAchieveOrange
                return new SolidColorBrush(Color.FromRgb(255, 140, 0));
            }
            else if (totalActiveHours <= 12)
            {
                // AchievedDarkGreen
                return new SolidColorBrush(Color.FromRgb(0, 100, 0));
            }
            else
            {
                // >12 hours: Gradient fill (Green bottom, Red top)
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 1);
                gradientBrush.EndPoint = new Point(0, 0);
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Green, 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                return gradientBrush;
            }
        }

        private string FormatSummaryLabel(double totalSeconds)
        {
            var hours = (int)(totalSeconds / 3600);
            var minutes = (int)((totalSeconds % 3600) / 60);
            var seconds = (int)(totalSeconds % 60);
            
            if (minutes == 0 && seconds == 0)
            {
                return $"{hours}h";
            }
            else
            {
                return $"{hours}.{minutes:D2}h";
            }
        }

        private string FormatDetailedTooltip(double totalSeconds)
        {
            var hours = (int)(totalSeconds / 3600);
            var minutes = (int)((totalSeconds % 3600) / 60);
            var seconds = (int)(totalSeconds % 60);
            
            return $"{hours:D2} hrs {minutes:D2} mins {seconds:D2} secs";
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
