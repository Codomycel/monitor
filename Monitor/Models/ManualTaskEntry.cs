using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemActivityTracker.Models
{
    public class ManualTaskEntry : INotifyPropertyChanged
    {
        private string _taskName = string.Empty;
        private int _totalSeconds;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string TaskName
        {
            get => _taskName;
            set
            {
                if (!string.Equals(_taskName, value, StringComparison.Ordinal))
                {
                    _taskName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalSeconds
        {
            get => _totalSeconds;
            set
            {
                if (_totalSeconds != value)
                {
                    _totalSeconds = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DurationText));
                }
            }
        }

        public string DurationText
        {
            get
            {
                var seconds = TotalSeconds;
                if (seconds < 0) seconds = 0;
                var ts = TimeSpan.FromSeconds(seconds);
                int hours = (int)ts.TotalHours;
                return string.Format("{0:00}:{1:00}:{2:00}", hours, ts.Minutes, ts.Seconds);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
