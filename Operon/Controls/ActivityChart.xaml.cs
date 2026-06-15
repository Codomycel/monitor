using System.Windows.Controls;
using SystemActivityTracker.ViewModels;

namespace SystemActivityTracker.Controls
{
    /// <summary>
    /// Interaction logic for ActivityChart.xaml
    /// </summary>
    public partial class ActivityChart : System.Windows.Controls.UserControl
    {
        public ActivityChart()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ActivityChartViewModel viewModel && ActualWidth > 0 && ActualHeight > 0)
            {
                viewModel.UpdateBarSizing(ActualWidth, ActualHeight);
            }
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // When the control is resized, notify the viewmodel to recalculate bar sizing
            if (DataContext is ActivityChartViewModel viewModel && e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                viewModel.UpdateBarSizing(e.NewSize.Width, e.NewSize.Height);
            }
        }
    }
}
