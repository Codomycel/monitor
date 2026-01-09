using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using SystemActivityTracker.Models;
using SystemActivityTracker.Views;

namespace SystemActivityTracker.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly App _app;
        private readonly TrackingService _trackingService;
        private readonly NotifyIcon _notifyIcon;
        private bool _disposed;

        public TrayIconService(App app, TrackingService trackingService)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));

            static string GetString(string key, string fallback)
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

            _notifyIcon = new NotifyIcon
            {
                Text = GetString("AppName", "System Activity Tracker"),
                Icon = GetAppIcon(),
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(GetString("TrayMenuOpen", "Open"), null, (_, __) => ShowMainWindow());
            contextMenu.Items.Add(GetString("TrayMenuExit", "Exit"), null, (_, __) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (_, __) => ShowMainWindow();
        }

        private static System.Drawing.Icon GetAppIcon()
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(exePath))
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                    if (icon != null)
                    {
                        return icon;
                    }
                }
            }
            catch
            {
            }

            return System.Drawing.SystemIcons.Application;
        }

        private void ShowMainWindow()
        {
            _app.Dispatcher.Invoke(() =>
            {
                if (_app.MainWindow == null)
                {
                    try
                    {
                        var settings = _app.SettingsService?.Load() ?? new AppSettings();
                        _app.MainWindow = _app.CreateMainWindowForMode(settings.UiMode, null);
                    }
                    catch
                    {
                        _app.MainWindow = new MainWindow();
                    }
                }

                if (_app.MainWindow is MainWindow mw)
                {
                    mw.RestoreFromTrayInternal();
                }
                else if (_app.MainWindow is ClassicMainWindow cmw)
                {
                    cmw.RestoreFromTrayInternal();
                }
                else
                {
                    _app.MainWindow.ShowInTaskbar = true;

                    if (!_app.MainWindow.IsVisible)
                    {
                        _app.MainWindow.Show();
                    }

                    if (_app.MainWindow.WindowState == WindowState.Minimized)
                    {
                        _app.MainWindow.WindowState = WindowState.Normal;
                    }

                    _app.MainWindow.Activate();
                }
            });
        }

        private void ExitApplication()
        {
            _app.Dispatcher.Invoke(() =>
            {
                if (_app.MainWindow is MainWindow mw)
                {
                    mw.RunRefreshCommandInternal();
                }
                else if (_app.MainWindow is ClassicMainWindow cmw)
                {
                    cmw.RunRefreshCommandInternal();
                }

                _app.IsShuttingDown = true;
                _app.Shutdown();
            });
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
