using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace SystemActivityTracker.Converters
{
    public sealed class SelectedDayMaxMinutesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return 0d;
            }

            double max = 0d;
            foreach (var v in values)
            {
                var minutes = DurationTextToMinutesConverter.ParseMinutes(v);
                if (minutes > max)
                {
                    max = minutes;
                }
            }

            return max;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class SelectedDayBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return 0d;
            }

            var valueMinutes = DurationTextToMinutesConverter.ParseMinutes(values[0]);

            double maxMinutes = 0d;
            if (values[1] is double d)
            {
                maxMinutes = d;
            }
            else if (values[1] != null && double.TryParse(values[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedMax))
            {
                maxMinutes = parsedMax;
            }

            double totalWidth = 0d;
            if (values[2] is double w)
            {
                totalWidth = w;
            }
            else if (values[2] != null && double.TryParse(values[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedWidth))
            {
                totalWidth = parsedWidth;
            }

            if (maxMinutes <= 0d || totalWidth <= 0d || valueMinutes <= 0d)
            {
                return 0d;
            }

            var ratio = valueMinutes / maxMinutes;
            if (ratio < 0d) ratio = 0d;
            if (ratio > 1d) ratio = 1d;

            return totalWidth * ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class GreaterThanZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = 0d;
            if (value is double d)
            {
                number = d;
            }
            else if (value != null && double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                number = parsed;
            }

            return number > 0d ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = 0d;
            if (value is double d)
            {
                number = d;
            }
            else if (value != null && double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                number = parsed;
            }

            return number <= 0d ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class DurationTextToMinutesConverter : IValueConverter
    {
        private static readonly Regex NumberRegex = new Regex(@"(\d+)", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ParseMinutes(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        internal static double ParseMinutes(object value)
        {
            if (value == null)
            {
                return 0d;
            }

            var text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0d;
            }

            var numbers = NumberRegex.Matches(text).Cast<Match>().Select(m => m.Value).ToArray();
            if (numbers.Length == 0)
            {
                return 0d;
            }

            var hours = 0;
            var minutes = 0;

            _ = int.TryParse(numbers[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out hours);
            if (numbers.Length > 1)
            {
                _ = int.TryParse(numbers[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes);
            }

            if (hours < 0) hours = 0;
            if (minutes < 0) minutes = 0;

            return (hours * 60d) + minutes;
        }
    }

    public sealed class DecimalHoursConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var totalMinutes = DurationTextToMinutesConverter.ParseMinutes(value);
            var hours = (int)(totalMinutes / 60);
            var minutes = (int)(totalMinutes % 60);
            return $"{hours}.{minutes:D2}h";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class Subtract10Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue - 10;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class Subtract8Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue - 8;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
