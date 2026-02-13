using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using SystemActivityTracker.Utilities;

namespace SystemActivityTracker.ViewModels
{
    public class ActivityChartViewModel : INotifyPropertyChanged
    {
        #region Input Properties (Data Source)
        
        /// <summary>
        /// Total active time for the selected period
        /// </summary>
        public TimeSpan TotalActiveTime { get; set; }

        /// <summary>
        /// Manual tasks duration for the selected period
        /// </summary>
        public TimeSpan ManualTasksDuration { get; set; }

        /// <summary>
        /// Idle time for the selected period
        /// </summary>
        public TimeSpan IdleTime { get; set; }

        /// <summary>
        /// Locked time for the selected period
        /// </summary>
        public TimeSpan LockedTime { get; set; }

        /// <summary>
        /// Reference time for 8-hour line (default: 8 hours)
        /// </summary>
        public TimeSpan ReferenceTime { get; set; } = TimeSpan.FromHours(8);

        /// <summary>
        /// Controls whether to show the reference line label (e.g., "8h")
        /// </summary>
        public bool ShowReferenceLabel { get; set; } = true;

        #endregion

        #region Output Properties (UI Bindings)

        private SeriesCollection _chartSeries = new SeriesCollection();
        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            private set
            {
                _chartSeries = value;
                OnPropertyChanged();
            }
        }

        private string[] _xAxisLabels = { "Total Active", "Locked", "Idle" };
        public string[] XAxisLabels
        {
            get => _xAxisLabels;
            private set
            {
                _xAxisLabels = value;
                OnPropertyChanged();
            }
        }

        private Func<double, string> _yAxisFormatter = value => $"{value / 3600.0:F1}h";
        public Func<double, string> YAxisFormatter
        {
            get => _yAxisFormatter;
            private set
            {
                _yAxisFormatter = value;
                OnPropertyChanged();
            }
        }

        private double _yAxisMax = 10;
        public double YAxisMax
        {
            get => _yAxisMax;
            private set
            {
                _yAxisMax = value;
                OnPropertyChanged();
            }
        }

        private double _eightHourLinePosition;
        public double EightHourLinePosition
        {
            get => _eightHourLinePosition;
            private set
            {
                _eightHourLinePosition = value;
                OnPropertyChanged();
            }
        }

        private double _eightHourLabelPosition;
        public double EightHourLabelPosition
        {
            get => _eightHourLabelPosition;
            private set
            {
                _eightHourLabelPosition = value;
                OnPropertyChanged();
            }
        }

        private double _eightHourLabelTextPosition;
        public double EightHourLabelTextPosition
        {
            get => _eightHourLabelTextPosition;
            private set
            {
                _eightHourLabelTextPosition = value;
                OnPropertyChanged();
            }
        }

        private string _referenceLabelText = "8h";
        public string ReferenceLabelText
        {
            get => _referenceLabelText;
            private set
            {
                _referenceLabelText = value;
                OnPropertyChanged();
            }
        }

        // Tooltip properties
        private string _tooltipTotalActive = "";
        public string TooltipTotalActive
        {
            get => _tooltipTotalActive;
            private set
            {
                _tooltipTotalActive = value;
                OnPropertyChanged();
            }
        }

        private string _tooltipActive = "";
        public string TooltipActive
        {
            get => _tooltipActive;
            private set
            {
                _tooltipActive = value;
                OnPropertyChanged();
            }
        }

        private string _tooltipManual = "";
        public string TooltipManual
        {
            get => _tooltipManual;
            private set
            {
                _tooltipManual = value;
                OnPropertyChanged();
            }
        }

        private string _tooltipIdle = "";
        public string TooltipIdle
        {
            get => _tooltipIdle;
            private set
            {
                _tooltipIdle = value;
                OnPropertyChanged();
            }
        }

        private string _tooltipLocked = "";
        public string TooltipLocked
        {
            get => _tooltipLocked;
            private set
            {
                _tooltipLocked = value;
                OnPropertyChanged();
            }
        }

        // Computed properties for UI binding
        public string TotalActiveText => TotalActiveTime.ToHoursMinutes();
        public string ManualTasksText => ManualTasksDuration.ToHoursMinutes();
        public string TotalActiveWithManualText => (TotalActiveTime + ManualTasksDuration).ToHoursMinutes();
        public string IdleText => IdleTime.ToHoursMinutes();
        public string LockedText => LockedTime.ToHoursMinutes();

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the chart with new data
        /// </summary>
        public void UpdateChart()
        {
            InitializeChartIfNeeded();
            UpdateChartData();
            UpdateTooltipData();
        }

        /// <summary>
        /// Sets all data values at once and updates the chart
        /// </summary>
        public void SetData(TimeSpan totalActive, TimeSpan manualTasks, TimeSpan idle, TimeSpan locked)
        {
            TotalActiveTime = totalActive;
            ManualTasksDuration = manualTasks;
            IdleTime = idle;
            LockedTime = locked;
            UpdateChart();
        }

        #endregion

        #region Private Methods

        private void InitializeChartIfNeeded()
        {
            if (ChartSeries.Count == 0)
            {
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
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)) // #6B7280 (Neutral Grey)
                };

                // Idle series (index 2)
                var idleSeries = new ColumnSeries
                {
                    Title = "Idle",
                    Values = new ChartValues<double> { 0.0 },
                    DataLabels = true,
                    StrokeThickness = 0,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 163, 175)) // #9CA3AF (Light Grey)
                };

                ChartSeries.Add(totalActiveSeries);
                ChartSeries.Add(lockedSeries);
                ChartSeries.Add(idleSeries);
            }
        }

        private void UpdateChartData()
        {
            if (ChartSeries.Count < 3) return;

            // Use seconds as the base unit for all calculations
            var totalActiveSeconds = (TotalActiveTime + ManualTasksDuration).TotalSeconds;
            var lockedSeconds = LockedTime.TotalSeconds;
            var idleSeconds = IdleTime.TotalSeconds;

            // Calculate Y-axis max with 10% padding
            var maxValue = Math.Max(totalActiveSeconds, Math.Max(lockedSeconds, idleSeconds));
            var referenceSeconds = ReferenceTime.TotalSeconds;
            YAxisMax = Math.Max(referenceSeconds, maxValue) * 1.1;

            // Calculate reference line position (in chart coordinates)
            var chartHeight = 180.0; // Chart area is approximately 180px tall
            EightHourLinePosition = chartHeight - (referenceSeconds / YAxisMax * chartHeight);
            EightHourLabelPosition = EightHourLinePosition - 10;
            EightHourLabelTextPosition = EightHourLinePosition - 8;

            // Update reference label text based on ReferenceTime
            var referenceHours = ReferenceTime.TotalHours;
            ReferenceLabelText = $"{referenceHours:F0}h";

            // Update Total Active series (index 0)
            if (ChartSeries[0].Values.Count > 0)
            {
                ChartSeries[0].Values[0] = totalActiveSeconds;
                // Apply conditional coloring to Total Active bar
                if (ChartSeries[0] is ColumnSeries totalActiveSeries)
                {
                    totalActiveSeries.Fill = GetTotalActiveColor(totalActiveSeconds / 60.0); // Convert to minutes for color calculation
                    totalActiveSeries.LabelPoint = point => FormatSummaryLabel(totalActiveSeconds);
                }
            }

            // Update Locked series (index 1)
            if (ChartSeries[1].Values.Count > 0)
            {
                ChartSeries[1].Values[0] = lockedSeconds;
                if (ChartSeries[1] is ColumnSeries lockedSeries)
                {
                    lockedSeries.LabelPoint = point => FormatSummaryLabel(lockedSeconds);
                }
            }

            // Update Idle series (index 2)
            if (ChartSeries[2].Values.Count > 0)
            {
                ChartSeries[2].Values[0] = idleSeconds;
                if (ChartSeries[2] is ColumnSeries idleSeries)
                {
                    idleSeries.LabelPoint = point => FormatSummaryLabel(idleSeconds);
                }
            }

            // Notify property changes for computed properties
            OnPropertyChanged(nameof(TotalActiveText));
            OnPropertyChanged(nameof(ManualTasksText));
            OnPropertyChanged(nameof(TotalActiveWithManualText));
            OnPropertyChanged(nameof(IdleText));
            OnPropertyChanged(nameof(LockedText));
        }

        private void UpdateTooltipData()
        {
            var totalActiveWithManual = TotalActiveTime + ManualTasksDuration;
            
            TooltipTotalActive = $"Total Active: {totalActiveWithManual.ToHoursMinutes()}";
            TooltipActive = $"Active (tracked): {TotalActiveTime.ToHoursMinutes()}";
            TooltipManual = $"Manual: {ManualTasksDuration.ToHoursMinutes()}";
            TooltipIdle = $"Idle: {IdleTime.ToHoursMinutes()}";
            TooltipLocked = $"Locked: {LockedTime.ToHoursMinutes()}";
        }

        private System.Windows.Media.Brush GetTotalActiveColor(double totalActiveMinutes)
        {
            var referenceHours = ReferenceTime.TotalHours;
            var hours = totalActiveMinutes / 60.0;
            
            // Clamp to cap (1.25x reference time)
            var capHours = referenceHours * 1.25;
            if (hours >= capHours)
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)); // #10B981 (Green)
            
            // Define color stops as percentages of reference time
            var colorStops = new[]
            {
                new { Hours = referenceHours * 0.25, Color = (R: (byte)239, G: (byte)68, B: (byte)68) },     // 25% = Red
                new { Hours = referenceHours * 0.5, Color = (R: (byte)251, G: (byte)146, B: (byte)60) },    // 50% = Orange
                new { Hours = referenceHours * 0.75, Color = (R: (byte)250, G: (byte)204, B: (byte)21) },   // 75% = Yellow
                new { Hours = referenceHours, Color = (R: (byte)34, G: (byte)197, B: (byte)94) },          // 100% = Light Green
                new { Hours = capHours, Color = (R: (byte)16, G: (byte)185, B: (byte)129) }               // 125% = Green
            };
            
            // Find the appropriate segment and interpolate
            for (int i = 0; i < colorStops.Length - 1; i++)
            {
                var start = colorStops[i];
                var end = colorStops[i + 1];
                
                if (hours >= start.Hours && hours <= end.Hours)
                {
                    // At exact boundary, return exact color
                    if (Math.Abs(hours - start.Hours) < 0.001)
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(start.Color.R, start.Color.G, start.Color.B));
                    
                    if (Math.Abs(hours - end.Hours) < 0.001)
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(end.Color.R, end.Color.G, end.Color.B));
                    
                    // Interpolate between colors
                    var t = (hours - start.Hours) / (end.Hours - start.Hours);
                    var interpolatedColor = InterpolateRgb(start.Color, end.Color, t);
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(interpolatedColor.R, interpolatedColor.G, interpolatedColor.B));
                }
            }
            
            // Below minimum, return red
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)); // #EF4444
        }
        
        private static (byte R, byte G, byte B) InterpolateRgb((byte R, byte G, byte B) start, (byte R, byte G, byte B) end, double t)
        {
            // Clamp t to [0, 1] range
            t = Math.Max(0, Math.Min(1, t));
            
            var r = (byte)(start.R + (end.R - start.R) * t);
            var g = (byte)(start.G + (end.G - start.G) * t);
            var b = (byte)(start.B + (end.B - start.B) * t);
            
            return (r, g, b);
        }

        private string FormatSummaryLabel(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            var hours = (int)timeSpan.TotalHours;
            var mins = timeSpan.Minutes;
            return $"{hours}.{mins:D2}h";
        }

        #endregion
    }
}
