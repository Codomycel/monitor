using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SystemActivityTracker.Utilities;
using System.Windows.Media;

namespace SystemActivityTracker.ViewModels
{
    /// <summary>
    /// ViewModel for a horizontal stacked activity bar showing Active, Manual, Idle, and Locked time segments.
    /// Designed for compact display in monthly calendar cells.
    /// Uses the same color logic as ActivityChartViewModel.
    /// </summary>
    public class HorizontalActivityBarViewModel : INotifyPropertyChanged
    {
        #region Input Properties

        private TimeSpan _activeDuration;
        public TimeSpan ActiveDuration
        {
            get => _activeDuration;
            set
            {
                if (_activeDuration != value)
                {
                    _activeDuration = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        private TimeSpan _manualDuration;
        public TimeSpan ManualDuration
        {
            get => _manualDuration;
            set
            {
                if (_manualDuration != value)
                {
                    _manualDuration = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        private TimeSpan _idleDuration;
        public TimeSpan IdleDuration
        {
            get => _idleDuration;
            set
            {
                if (_idleDuration != value)
                {
                    _idleDuration = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        private TimeSpan _lockedDuration;
        public TimeSpan LockedDuration
        {
            get => _lockedDuration;
            set
            {
                if (_lockedDuration != value)
                {
                    _lockedDuration = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        private TimeSpan _referenceTime = TimeSpan.FromHours(8);
        public TimeSpan ReferenceTime
        {
            get => _referenceTime;
            set
            {
                if (_referenceTime != value)
                {
                    _referenceTime = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        #endregion

        #region Computed Display Properties

        /// <summary>
        /// Total active time including manual tasks (Active + Manual)
        /// </summary>
        public TimeSpan TotalActiveDuration => ActiveDuration + ManualDuration;

        /// <summary>
        /// Total duration of all segments (for proportional sizing)
        /// </summary>
        public TimeSpan TotalDuration => ActiveDuration + ManualDuration + IdleDuration + LockedDuration;

        /// <summary>
        /// Whether there is any data to display (non-zero total)
        /// </summary>
        public bool HasData => TotalDuration > TimeSpan.Zero;

        /// <summary>
        /// Width proportion for Active segment (Star value)
        /// </summary>
        private double _activeWidth;
        public double ActiveWidth
        {
            get => _activeWidth;
            private set
            {
                if (Math.Abs(_activeWidth - value) > 0.001)
                {
                    _activeWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Width proportion for Manual segment (Star value)
        /// </summary>
        private double _manualWidth;
        public double ManualWidth
        {
            get => _manualWidth;
            private set
            {
                if (Math.Abs(_manualWidth - value) > 0.001)
                {
                    _manualWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Width proportion for Idle segment (Star value)
        /// </summary>
        private double _idleWidth;
        public double IdleWidth
        {
            get => _idleWidth;
            private set
            {
                if (Math.Abs(_idleWidth - value) > 0.001)
                {
                    _idleWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Width proportion for Locked segment (Star value)
        /// </summary>
        private double _lockedWidth;
        public double LockedWidth
        {
            get => _lockedWidth;
            private set
            {
                if (Math.Abs(_lockedWidth - value) > 0.001)
                {
                    _lockedWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to show Active segment
        /// </summary>
        public bool ShowActive => ActiveDuration > TimeSpan.Zero;

        /// <summary>
        /// Whether to show Manual segment
        /// </summary>
        public bool ShowManual => ManualDuration > TimeSpan.Zero;

        /// <summary>
        /// Whether to show Idle segment
        /// </summary>
        public bool ShowIdle => IdleDuration > TimeSpan.Zero;

        /// <summary>
        /// Whether to show Locked segment
        /// </summary>
        public bool ShowLocked => LockedDuration > TimeSpan.Zero;

        // Additional visibility properties for proper corner radius handling

        /// <summary>
        /// Manual is visible but Active is not (for left-rounded corners on Manual)
        /// </summary>
        public bool ShowManualNoActive => ShowManual && !ShowActive;

        /// <summary>
        /// Only Active is visible
        /// </summary>
        public bool ShowOnlyActive => ShowActive && !ShowManual && !ShowIdle && !ShowLocked;

        /// <summary>
        /// Only Manual is visible
        /// </summary>
        public bool ShowOnlyManual => !ShowActive && ShowManual && !ShowIdle && !ShowLocked;

        /// <summary>
        /// Only Idle is visible
        /// </summary>
        public bool ShowOnlyIdle => !ShowActive && !ShowManual && ShowIdle && !ShowLocked;

        /// <summary>
        /// Only Locked is visible
        /// </summary>
        public bool ShowOnlyLocked => !ShowActive && !ShowManual && !ShowIdle && ShowLocked;

        /// <summary>
        /// Idle is the first visible segment (no Active, no Manual)
        /// </summary>
        public bool ShowIdleFirst => !ShowActive && !ShowManual && ShowIdle;

        /// <summary>
        /// Locked is the first visible segment
        /// </summary>
        public bool ShowLockedFirst => !ShowActive && !ShowManual && !ShowIdle && ShowLocked;

        /// <summary>
        /// Manual should have right-rounded corners (when Manual is last visible segment)
        /// </summary>
        public bool ShowManualEnd => ShowManual && !ShowIdle && !ShowLocked;

        /// <summary>
        /// Idle should have right-rounded corners (when Idle is last visible segment)
        /// </summary>
        public bool ShowIdleEnd => ShowIdle && !ShowLocked;

        /// <summary>
        /// Locked always has right-rounded corners when visible with other segments
        /// </summary>
        public bool ShowLockedAlwaysEnd => ShowLocked && (ShowActive || ShowManual || ShowIdle);

        #endregion

        #region Colors (same as ActivityChartViewModel)

        /// <summary>
        /// Brush for Active segment - uses same gradient logic as ActivityChartViewModel
        /// </summary>
        private System.Windows.Media.Brush _activeBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)); // #EF4444 default
        public System.Windows.Media.Brush ActiveBrush
        {
            get => _activeBrush;
            private set
            {
                _activeBrush = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Brush for Manual segment - uses same amber as UiB selected day chart
        /// </summary>
        public System.Windows.Media.Brush ManualBrush { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 158, 11)); // #F59E0B

        /// <summary>
        /// Brush for Idle segment - same as ActivityChartViewModel
        /// </summary>
        public System.Windows.Media.Brush IdleBrush { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 163, 175)); // #9CA3AF

        /// <summary>
        /// Brush for Locked segment - same as ActivityChartViewModel
        /// </summary>
        public System.Windows.Media.Brush LockedBrush { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)); // #6B7280

        /// <summary>
        /// Background brush for empty state
        /// </summary>
        public System.Windows.Media.Brush EmptyBackgroundBrush { get; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 231, 235)); // #E5E7EB

        #endregion

        #region Tooltip Properties

        public string TooltipTotalActive => $"Total Active: {TotalActiveDuration.ToHoursMinutes()}";
        public string TooltipActive => $"Active: {ActiveDuration.ToHoursMinutes()}";
        public string TooltipManual => $"Manual Tasks: {ManualDuration.ToHoursMinutes()}";
        public string TooltipIdle => $"Idle: {IdleDuration.ToHoursMinutes()}";
        public string TooltipLocked => $"Locked: {LockedDuration.ToHoursMinutes()}";

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets all durations at once and triggers recalculation
        /// </summary>
        public void SetData(TimeSpan active, TimeSpan manual, TimeSpan idle, TimeSpan locked)
        {
            _activeDuration = active;
            _manualDuration = manual;
            _idleDuration = idle;
            _lockedDuration = locked;

            OnPropertyChanged(nameof(ActiveDuration));
            OnPropertyChanged(nameof(ManualDuration));
            OnPropertyChanged(nameof(IdleDuration));
            OnPropertyChanged(nameof(LockedDuration));

            Recalculate();
        }

        #endregion

        #region Private Methods

        private void Recalculate()
        {
            var total = TotalDuration;
            if (total > TimeSpan.Zero)
            {
                // Use seconds as the proportional width value
                ActiveWidth = ActiveDuration.TotalSeconds;
                ManualWidth = ManualDuration.TotalSeconds;
                IdleWidth = IdleDuration.TotalSeconds;
                LockedWidth = LockedDuration.TotalSeconds;
            }
            else
            {
                // No data - all segments zero width
                ActiveWidth = 0;
                ManualWidth = 0;
                IdleWidth = 0;
                LockedWidth = 0;
            }

            // Update visibility properties
            OnPropertyChanged(nameof(ShowActive));
            OnPropertyChanged(nameof(ShowManual));
            OnPropertyChanged(nameof(ShowIdle));
            OnPropertyChanged(nameof(ShowLocked));
            OnPropertyChanged(nameof(HasData));

            // Update corner radius visibility properties
            OnPropertyChanged(nameof(ShowManualNoActive));
            OnPropertyChanged(nameof(ShowOnlyActive));
            OnPropertyChanged(nameof(ShowOnlyManual));
            OnPropertyChanged(nameof(ShowOnlyIdle));
            OnPropertyChanged(nameof(ShowOnlyLocked));
            OnPropertyChanged(nameof(ShowIdleFirst));
            OnPropertyChanged(nameof(ShowLockedFirst));
            OnPropertyChanged(nameof(ShowManualEnd));
            OnPropertyChanged(nameof(ShowIdleEnd));
            OnPropertyChanged(nameof(ShowLockedAlwaysEnd));

            // Update tooltip properties
            OnPropertyChanged(nameof(TooltipTotalActive));
            OnPropertyChanged(nameof(TooltipActive));
            OnPropertyChanged(nameof(TooltipManual));
            OnPropertyChanged(nameof(TooltipIdle));
            OnPropertyChanged(nameof(TooltipLocked));

            // Update active color based on total active vs reference
            UpdateActiveColor();
        }

        /// <summary>
        /// Updates the Active color based on total active time vs reference time.
        /// Uses the exact same logic as ActivityChartViewModel.GetTotalActiveColor()
        /// </summary>
        private void UpdateActiveColor()
        {
            var totalActiveMinutes = TotalActiveDuration.TotalMinutes;
            var referenceHours = ReferenceTime.TotalHours;
            var hours = totalActiveMinutes / 60.0;

            var color = GetActiveColorForHours(hours, referenceHours);
            ActiveBrush = new SolidColorBrush(color);
        }

        /// <summary>
        /// Gets the active color based on hours worked relative to reference hours.
        /// Mirrors the logic in ActivityChartViewModel.GetTotalActiveColor()
        /// </summary>
        private static System.Windows.Media.Color GetActiveColorForHours(double hours, double referenceHours)
        {
            var capHours = referenceHours * 1.25;

            // At or above cap = Green (#10B981)
            if (hours >= capHours)
                return System.Windows.Media.Color.FromRgb(16, 185, 129);

            // Define color stops as percentages of reference time
            var colorStops = new[]
            {
                new { Hours = referenceHours * 0.25, R = (byte)239, G = (byte)68, B = (byte)68 },     // Red #EF4444
                new { Hours = referenceHours * 0.50, R = (byte)251, G = (byte)146, B = (byte)60 },   // Orange #FBA73C
                new { Hours = referenceHours * 0.75, R = (byte)250, G = (byte)204, B = (byte)21 },    // Yellow #FACD15
                new { Hours = referenceHours * 1.00, R = (byte)34, G = (byte)197, B = (byte)94 },    // Light Green #22C55E
                new { Hours = capHours, R = (byte)16, G = (byte)185, B = (byte)129 }                // Green #10B981
            };

            // Find the appropriate segment and interpolate
            for (int i = 0; i < colorStops.Length - 1; i++)
            {
                var start = colorStops[i];
                var end = colorStops[i + 1];

                if (hours >= start.Hours && hours <= end.Hours)
                {
                    if (Math.Abs(hours - start.Hours) < 0.001)
                        return System.Windows.Media.Color.FromRgb(start.R, start.G, start.B);
                    if (Math.Abs(hours - end.Hours) < 0.001)
                        return System.Windows.Media.Color.FromRgb(end.R, end.G, end.B);

                    var t = (hours - start.Hours) / (end.Hours - start.Hours);
                    return InterpolateColor(start.R, start.G, start.B, end.R, end.G, end.B, t);
                }
            }

            // Below minimum, return red
            return System.Windows.Media.Color.FromRgb(239, 68, 68);
        }

        private static System.Windows.Media.Color InterpolateColor(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return System.Windows.Media.Color.FromRgb(
                (byte)(r1 + (r2 - r1) * t),
                (byte)(g1 + (g2 - g1) * t),
                (byte)(b1 + (b2 - b1) * t));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
