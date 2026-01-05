using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SystemActivityTracker.Services;
using System.Windows.Threading;

namespace SystemActivityTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private SessionStateService? _sessionStateService;
        private TrackingService? _trackingService;
        private TrayIconService? _trayIconService;
        private SettingsService? _settingsService;
        private CloseTrackingService? _closeTrackingService;
        private DispatcherTimer? _uiHeartbeatTimer;

        public bool IsShuttingDown { get; set; }

        public CloseTrackingService? CloseTrackingService => _closeTrackingService;

        public TrackingService? TrackingService => _trackingService;
        public SettingsService? SettingsService => _settingsService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settingsService = new SettingsService();
            var settings = _settingsService.Load();

            _closeTrackingService = new CloseTrackingService();
            _closeTrackingService.ApplyCrashLogPolicy(settings.CrashLogRetentionDays, settings.CrashLogMaxSizeMB);
            _closeTrackingService.CleanupCrashLogs();
            RegisterCloseTrackingHooks();
            StartUiHeartbeat();

            _sessionStateService = new SessionStateService();
            _trackingService = new TrackingService(_sessionStateService, _settingsService);
            if (_trackingService != null)
            {
                _trayIconService = new TrayIconService(this, _trackingService);
            }

            SystemEvents.SessionEnding += OnSystemSessionEnding;
        }

        private void RegisterCloseTrackingHooks()
        {
            DispatcherUnhandledException += (_, exArgs) =>
            {
                try
                {
                    _closeTrackingService?.MarkUnhandledUiException(exArgs.Exception);
                }
                catch
                {
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (_, exArgs) =>
            {
                try
                {
                    if (exArgs.ExceptionObject is Exception ex)
                    {
                        _closeTrackingService?.MarkUnhandledBackgroundException(ex);
                    }
                }
                catch
                {
                }
            };

            TaskScheduler.UnobservedTaskException += (_, exArgs) =>
            {
                try
                {
                    _closeTrackingService?.MarkUnobservedTaskException(exArgs.Exception);
                    exArgs.SetObserved();
                }
                catch
                {
                }
            };
        }

        private void StartUiHeartbeat()
        {
            _uiHeartbeatTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _uiHeartbeatTimer.Tick += (_, __) =>
            {
                try
                {
                    _closeTrackingService?.TouchUiHeartbeat();
                }
                catch
                {
                }
            };
            _uiHeartbeatTimer.Start();
        }

        private void OnSystemSessionEnding(object? sender, SessionEndingEventArgs e)
        {
            try
            {
                if (_closeTrackingService != null)
                {
                    _closeTrackingService.ShutdownOrLogoffDetected = true;
                }
                _trackingService?.HandleSessionEnding();
            }
            catch
            {
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemEvents.SessionEnding -= OnSystemSessionEnding;

            if (_uiHeartbeatTimer != null)
            {
                _uiHeartbeatTimer.Stop();
                _uiHeartbeatTimer = null;
            }

            try
            {
                _closeTrackingService?.CompleteGracefulExit();
            }
            catch
            {
            }
            finally
            {
                _closeTrackingService?.Dispose();
                _closeTrackingService = null;
            }

            _trayIconService?.Dispose();
            _trayIconService = null;
            _trackingService?.Shutdown();
            _trackingService?.Dispose();
            _trackingService = null;
            _settingsService = null;
            _sessionStateService?.Dispose();
            _sessionStateService = null;
            base.OnExit(e);
        }
    }

}
