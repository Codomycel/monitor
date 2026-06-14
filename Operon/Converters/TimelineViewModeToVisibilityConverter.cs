using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SystemActivityTracker.ViewModels;

namespace SystemActivityTracker.Converters
{
    /// <summary>
    /// Converts TimelineViewMode to Visibility - shows timeline for Week/Month modes, hides for Date mode
    /// Usage: Visibility="{Binding TimelineViewMode, Converter={StaticResource TimelineViewModeToVisibilityConverter}}"
    /// </summary>
    public class TimelineViewModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimelineViewMode viewMode)
            {
                // Show timeline for Week and Month modes, hide for Date mode
                return (viewMode == TimelineViewMode.Week || viewMode == TimelineViewMode.Month) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
