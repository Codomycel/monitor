using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SystemActivityTracker.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility with inverted logic
    /// True -> Collapsed, False -> Visible
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // Default to Visible for null or non-boolean values
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            
            return false;
        }
    }
}
