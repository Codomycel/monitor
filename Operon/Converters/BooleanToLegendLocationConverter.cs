using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemActivityTracker.Converters
{
    /// <summary>
    /// Converts a boolean value to LegendLocation string
    /// True -> Bottom (show legend), False -> None (hide legend)
    /// </summary>
    public class BooleanToLegendLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showLegend)
            {
                return showLegend ? "Bottom" : "None";
            }
            
            // Default to showing legend
            return "Bottom";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string location)
            {
                return location != "None";
            }
            
            return true;
        }
    }
}
