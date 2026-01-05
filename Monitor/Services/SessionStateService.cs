using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace SystemActivityTracker.Services
{
    public sealed class SessionLockChangedEventArgs : EventArgs
    {
        public bool IsLocked { get; }
        public DateTime Timestamp { get; }
        public SessionSwitchReason Reason { get; }

        public SessionLockChangedEventArgs(bool isLocked, DateTime timestamp, SessionSwitchReason reason)
        {
            IsLocked = isLocked;
            Timestamp = timestamp;
            Reason = reason;
        }
    }

    public class SessionStateService : IDisposable
    {
        private bool _isDisposed;

        public bool IsLocked { get; private set; }

        public event EventHandler<bool>? LockStateChanged;
        public event EventHandler<SessionLockChangedEventArgs>? LockEvent;

        public SessionStateService()
        {
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }

        private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    UpdateLockState(true, e.Reason);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    UpdateLockState(false, e.Reason);
                    break;
            }
        }

        private void UpdateLockState(bool isLocked, SessionSwitchReason reason)
        {
            if (IsLocked == isLocked)
            {
                return;
            }

            IsLocked = isLocked;
            LockStateChanged?.Invoke(this, IsLocked);

            var now = DateTime.Now;
            Debug.WriteLine($"[Session] {(IsLocked ? "LOCK" : "UNLOCK")} at {now:o} (Reason={reason})");
            LockEvent?.Invoke(this, new SessionLockChangedEventArgs(IsLocked, now, reason));
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        ~SessionStateService()
        {
            Dispose();
        }
    }
}
