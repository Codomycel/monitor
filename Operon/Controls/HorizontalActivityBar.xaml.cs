using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemActivityTracker.Controls
{
    /// <summary>
    /// Interaction logic for HorizontalActivityBar.xaml
    /// A horizontal stacked bar control showing Active, Manual, Idle, and Locked time segments.
    /// </summary>
    public partial class HorizontalActivityBar : System.Windows.Controls.UserControl
    {
        public HorizontalActivityBar()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Converts a double ratio to a GridLength with Star sizing.
    /// Example: 0.375 becomes "0.375*" for 37.5% proportional width.
    /// </summary>
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ratio && ratio > 0)
            {
                // Create GridLength with Star sizing: ratio*
                // Example: 0.375 becomes 0.375* (37.5% of available space)
                return new System.Windows.GridLength(ratio, System.Windows.GridUnitType.Star);
            }
            // Return 0* for zero ratio (no space)
            return new System.Windows.GridLength(0, System.Windows.GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
