using System;
using System.Threading;
using System.Diagnostics;

namespace SystemActivityTracker.Utilities
{
    /// <summary>
    /// Manages single instance enforcement for the application using a named mutex.
    /// This class prevents multiple instances of the application from running simultaneously
    /// by leveraging a named system mutex that is automatically released if the process terminates.
    /// </summary>
    /// <remarks>
    /// Thread Safety: This class uses System.Threading.Mutex which is thread-safe.
    /// The named mutex is system-wide and will be automatically released if the process terminates,
    /// even in case of an abnormal crash.
    /// 
    /// Usage:
    /// 1. Create an instance of SingleInstanceManager
    /// 2. Call AcquireInstance() to check if another instance is running
    /// 3. If it returns false, another instance owns the lock
    /// 4. If it returns true, this is the first instance and should continue
    /// 5. Keep the manager instance alive for the application lifetime
    /// 6. Call Dispose() when the application exits to release the mutex
    /// </remarks>
    public class SingleInstanceManager : IDisposable
    {
        /// <summary>
        /// The unique mutex name used for instance detection. 
        /// Uses a Global namespace to ensure system-wide detection across all user sessions.
        /// </summary>
        private const string MutexName = "Global\\Operon_SingleInstance_28F6C6A2-D4F9-48F5-9C6A-1A8B3C5E7D9F";

    /// <summary>
    /// The unique event name used to notify the first instance that a second instance
    /// attempted to start. Also placed in the Global namespace to allow signaling
    /// across sessions.
    /// </summary>
    private const string ActivationEventName = "Global\\Operon_SingleInstance_Activation_28F6C6A2-D4F9-48F5-9C6A-1A8B3C5E7D9F";

    /// <summary>
    /// The mutex instance used for synchronization. Stored in a static field so that
    /// it is held for the lifetime of the process regardless of the lifetime of
    /// any particular SingleInstanceManager object.
    /// </summary>
    private static Mutex? _instanceMutex;

    /// <summary>
    /// The event handle used to signal an existing instance.
    /// </summary>
    private static EventWaitHandle? _activationEvent;

    /// <summary>
    /// Tracks whether Dispose has already run to avoid double-dispose.
    /// </summary>
    private bool _disposed;

        /// <summary>
        /// Gets a value indicating whether an instance is already running.
        /// This is set during AcquireInstance() call.
        /// </summary>
        /// <value>
        /// True if another instance already owns the mutex; false if this is the first instance.
        /// </value>
        public bool IsInstanceAlreadyRunning { get; private set; }

        /// <summary>
        /// Raised when a second instance tries to start; the first instance should
        /// activate its main window in response.
        /// </summary>
        public event EventHandler? InstanceActivated;

        /// <summary>
        /// Attempts to acquire the instance lock.
        /// </summary>
        /// <returns>
        /// True if this is the only instance and the mutex was successfully acquired; 
        /// false if another instance already owns the mutex.
        /// </returns>
        /// <remarks>
        /// This method should be called early in the application startup, before any UI or 
        /// critical resources are initialized. If this returns false, the application should 
        /// exit gracefully after attempting to activate the existing instance window.
        /// </remarks>
        public bool AcquireInstance()
        {
            try
            {
                // Attempt to create/open the named mutex. The static field ensures it
                // remains alive for the lifetime of the process.
                _instanceMutex = new Mutex(true, MutexName, out bool createdNewMutex);

                // Always create/open the activation event regardless of whether we are first or second.
                // Use AutoReset so that signalling wakes a single waiter and then resets itself.
                _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);

                if (!createdNewMutex)
                {
                    // Another instance already owns the mutex; signal it to activate.
                    IsInstanceAlreadyRunning = true;
                    try
                    {
                        if (_activationEvent != null)
                    {
                        _activationEvent.Set();
                        Debug.WriteLine("Signalled existing instance to activate");
                    }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error signalling existing instance: {ex.Message}");
                    }

                    // tidy up resources we created locally (the first instance still holds them)
                    _instanceMutex?.Dispose();
                    _instanceMutex = null;
                    _activationEvent?.Dispose();
                    _activationEvent = null;
                    return false;
                }

                // First instance succeeded in acquiring mutex; start listening for activation signals.
                IsInstanceAlreadyRunning = false;
                BeginWaitForActivation();
                return true;
            }
            catch (Exception ex)
            {
                // In case of any error, assume we can't acquire, so let another instance run
                Debug.WriteLine($"Error acquiring instance mutex: {ex.Message}");
                IsInstanceAlreadyRunning = true;
                return false;
            }
        }

        /// <summary>
        /// Releases the instance mutex and disposes the activation event.
        /// </summary>
        /// <remarks>
        /// This is called automatically by Dispose(). For proper cleanup, ensure Dispose() is called
        /// when the application exits. If the application crashes, Windows automatically releases the mutex.
        /// </remarks>
        public void ReleaseMutex()
        {
            try
            {
                if (_instanceMutex != null)
                {
                    _instanceMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error releasing instance mutex: {ex.Message}");
            }

            // the activation event does not need to be signalled on release; just dispose it
            try
            {
            if (_activationEvent != null)
            {
                _activationEvent.Dispose();
            }
        }
            catch { }
        }

        /// <summary>
        /// Releases all resources held by this SingleInstanceManager instance.
        /// </summary>
        /// <remarks>
        /// This should be called during application shutdown to properly release the mutex.
        /// Multiple calls to Dispose() are safe and will not throw an exception.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            ReleaseMutex();
            _instanceMutex?.Dispose();
            _instanceMutex = null;
            _activationEvent?.Dispose();
            _activationEvent = null;
            _disposed = true;
        }
        /// <summary>
        /// Begins waiting for an activation signal from a secondary instance.
        /// When the event is signalled we raise the <see cref="InstanceActivated"/> event.
        /// </summary>
        private void BeginWaitForActivation()
        {
            if (_activationEvent == null)
                return;

            // Register a wait callback that fires every time the event is signalled.
            ThreadPool.RegisterWaitForSingleObject(
                _activationEvent,
                (state, timedOut) => OnActivationSignalReceived(),
                null,
                Timeout.Infinite,
                executeOnlyOnce: false);
        }

        private void OnActivationSignalReceived()
        {
            try
            {
                Debug.WriteLine("Received activation signal from secondary instance");
                InstanceActivated?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // swallow exceptions from handlers
            }
        }
    }
}
