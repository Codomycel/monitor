using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SystemActivityTracker.Utilities;
using SystemActivityTracker.ViewModels;

namespace SystemActivityTracker.Views
{
    public partial class UiAMainWindow : Window
    {
        private bool _isExplicitExit;
        private bool _didInitialRefresh;
        private bool _isUiSwap;
        private bool _isInitializingUiMode;

        public UiAMainWindow()
        {
            InitializeComponent();

            if (System.Windows.Application.Current is App app)
            {
                try
                {
                    DataContext = app.Services.GetRequiredService<MainWindowViewModel>();
                }
                catch
                {
                    DataContext = new MainWindowViewModel(app.TrackingService, app.SettingsService);
                }

                try
                {
                    var settings = app.SettingsService?.Load();
                    if (settings != null)
                    {
                        _isInitializingUiMode = true;
                        SetComboSelection(UiModeComboBox, settings.UiMode);
                        SetComboSelection(UiModeComboBoxHeader, settings.UiMode);
                        _isInitializingUiMode = false;
                    }
                }
                catch
                {
                    _isInitializingUiMode = false;
                }
            }
            else
            {
                DataContext = new MainWindowViewModel(null, null);
            }

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_didInitialRefresh)
            {
                return;
            }

            _didInitialRefresh = true;
            RunRefreshCommand();
        }

        private void RunRefreshCommand()
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }

            ICommand command = vm.RefreshCommand;
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        }

        private void HideToTray()
        {
            RunRefreshCommand();
            ShowInTaskbar = false;
            Hide();
        }

        private void RestoreFromTray()
        {
            ShowInTaskbar = true;

            if (!IsVisible)
            {
                Show();
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }

        internal void RestoreFromTrayInternal()
        {
            RestoreFromTray();
        }

        internal void RunRefreshCommandInternal()
        {
            RunRefreshCommand();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RunRefreshCommand();
            _isExplicitExit = true;

            if (System.Windows.Application.Current is App app)
            {
                if (app.CloseTrackingService != null)
                {
                    app.CloseTrackingService.IsUserInitiatedExit = true;
                }
                app.IsShuttingDown = true;
                app.Shutdown();
            }
            else
            {
                System.Windows.Application.Current?.Shutdown();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_isExplicitExit || _isUiSwap)
            {
                return;
            }

            if (System.Windows.Application.Current is App app && !app.IsShuttingDown)
            {
                e.Cancel = true;
                HideToTray();
            }
        }

        private void UiModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializingUiMode)
            {
                return;
            }

            if (sender is not System.Windows.Controls.ComboBox combo)
            {
                return;
            }

            if (combo.SelectedItem is not System.Windows.Controls.ComboBoxItem item)
            {
                return;
            }

            if (item.Tag is not string mode || string.IsNullOrWhiteSpace(mode))
            {
                return;
            }

            if (!string.Equals(mode, UiModes.UIA, System.StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(mode, UiModes.UIB, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (System.Windows.Application.Current is not App app)
            {
                return;
            }

            var settingsService = app.SettingsService;
            if (settingsService == null)
            {
                return;
            }

            SystemActivityTracker.Models.AppSettings settings;
            try
            {
                settings = settingsService.Load();
            }
            catch
            {
                settings = new SystemActivityTracker.Models.AppSettings();
            }

            if (string.Equals(settings.UiMode, mode, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Keep both selectors in sync
            _isInitializingUiMode = true;
            try
            {
                if (!ReferenceEquals(combo, UiModeComboBox))
                {
                    SetComboSelection(UiModeComboBox, mode);
                }
                if (!ReferenceEquals(combo, UiModeComboBoxHeader))
                {
                    SetComboSelection(UiModeComboBoxHeader, mode);
                }
            }
            finally
            {
                _isInitializingUiMode = false;
            }

            settings.UiMode = mode;
            try
            {
                settingsService.Save(settings);
            }
            catch
            {
            }

            _isUiSwap = true;
            app.SwitchUiMode(mode, DataContext as MainWindowViewModel);
        }

        private static void SetComboSelection(System.Windows.Controls.ComboBox comboBox, string mode)
        {
            foreach (var obj in comboBox.Items)
            {
                if (obj is System.Windows.Controls.ComboBoxItem item && item.Tag is string tag &&
                    string.Equals(tag, mode, System.StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }
    }
}
