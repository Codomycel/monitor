using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SystemActivityTracker.Converters
{
    /// <summary>
    /// Converts an Enum value to Visibility based on whether it matches the parameter
    /// Usage: Visibility="{Binding MyEnumProperty, Converter={StaticResource EnumToVisibilityConverter}, ConverterParameter=EnumValue}"
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            var isMatch = value.ToString().Equals(parameter.ToString(), StringComparison.Ordinal);
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
