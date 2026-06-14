using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemActivityTracker.Converters
{
    /// <summary>
    /// Converts an Enum value to boolean for use with RadioButtons
    /// Usage: IsChecked="{Binding MyEnumProperty, Converter={StaticResource EnumToBoolConverter}, ConverterParameter=EnumValue}"
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString(), StringComparison.Ordinal);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                // Parse the enum value from the parameter
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, parameter.ToString());
                }
            }

            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
