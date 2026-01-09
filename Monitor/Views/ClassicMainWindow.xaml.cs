using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SystemActivityTracker.Models;
using SystemActivityTracker.ViewModels;

namespace SystemActivityTracker.Views
{
    public partial class ClassicMainWindow : Window
    {
        private bool _isExplicitExit;
        private bool _didInitialRefresh;
        private bool _isUiSwap;

        public ClassicMainWindow()
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
                        SetUiModeComboSelection(settings.UiMode);
                    }
                }
                catch
                {
                }
            }
            else
            {
                DataContext = new MainWindowViewModel(null, null);
            }

            Loaded += ClassicMainWindow_Loaded;
        }

        private void ClassicMainWindow_Loaded(object sender, RoutedEventArgs e)
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
            if (sender is not System.Windows.Controls.ComboBox combo)
            {
                return;
            }

            if (combo.SelectedItem is not System.Windows.Controls.ComboBoxItem item || item.Content is not string mode)
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

            AppSettings settings;
            try
            {
                settings = settingsService.Load();
            }
            catch
            {
                settings = new AppSettings();
            }

            if (string.Equals(settings.UiMode, mode, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
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

        private void SetUiModeComboSelection(string mode)
        {
            if (UiModeComboBox != null)
            {
                SetComboSelection(UiModeComboBox, mode);
            }
        }

        private static void SetComboSelection(System.Windows.Controls.ComboBox comboBox, string mode)
        {
            foreach (var obj in comboBox.Items)
            {
                if (obj is System.Windows.Controls.ComboBoxItem item && item.Content is string content &&
                    string.Equals(content, mode, System.StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }
    }
}
