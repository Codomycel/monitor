using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemActivityTracker.Models
{
    public class DailySummary : INotifyPropertyChanged
    {
        private DateTime _date;
        private TimeSpan _activeDuration;
        private TimeSpan _manualTaskDuration;
        private TimeSpan _idleDuration;
        private TimeSpan _lockedDuration;

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value)
                {
                    return;
                }

                _date = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ActiveDuration
        {
            get => _activeDuration;
            set => SetDuration(ref _activeDuration, value, nameof(ActiveDurationText));
        }

        public TimeSpan ManualTaskDuration
        {
            get => _manualTaskDuration;
            set => SetDuration(ref _manualTaskDuration, value, nameof(ManualTaskDurationText));
        }

        public TimeSpan IdleDuration
        {
            get => _idleDuration;
            set => SetDuration(ref _idleDuration, value, nameof(IdleDurationText));
        }

        public TimeSpan LockedDuration
        {
            get => _lockedDuration;
            set => SetDuration(ref _lockedDuration, value, nameof(LockedDurationText));
        }

        public TimeSpan TotalActiveDuration => ActiveDuration + ManualTaskDuration;

        public string ActiveDurationText => ActiveDuration.ToString(@"hh\:mm");
        public string ManualTaskDurationText => ManualTaskDuration.ToString(@"hh\:mm");
        public string TotalActiveDurationText => TotalActiveDuration.ToString(@"hh\:mm");
        public string IdleDurationText => IdleDuration.ToString(@"hh\:mm");
        public string LockedDurationText => LockedDuration.ToString(@"hh\:mm");

        public void SetDurations(TimeSpan active, TimeSpan manual, TimeSpan idle, TimeSpan locked)
        {
            ActiveDuration = active;
            ManualTaskDuration = manual;
            IdleDuration = idle;
            LockedDuration = locked;
        }

        private void SetDuration(ref TimeSpan field, TimeSpan value, string textPropertyName)
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(textPropertyName);
            OnPropertyChanged(nameof(TotalActiveDuration));
            OnPropertyChanged(nameof(TotalActiveDurationText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
