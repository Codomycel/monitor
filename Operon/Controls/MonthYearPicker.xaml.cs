using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SystemActivityTracker.Controls
{
    /// <summary>
    /// Month+Year picker - selects month and year only (no day).
    /// Displays "MMM yyyy" and opens a popup with month grid + year navigation.
    /// </summary>
    public partial class MonthYearPicker : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty SelectedMonthYearProperty =
            DependencyProperty.Register(
                nameof(SelectedMonthYear),
                typeof(DateTime),
                typeof(MonthYearPicker),
                new FrameworkPropertyMetadata(
                    DateTime.Today,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedMonthYearChanged));

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(
                nameof(DisplayText),
                typeof(string),
                typeof(MonthYearPicker),
                new PropertyMetadata(""));

        public DateTime SelectedMonthYear
        {
            get => (DateTime)GetValue(SelectedMonthYearProperty);
            set => SetValue(SelectedMonthYearProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            private set => SetValue(DisplayTextProperty, value);
        }

        private static void OnSelectedMonthYearChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MonthYearPicker picker)
            {
                picker.DisplayText = picker.SelectedMonthYear.ToString("MMM yyyy", CultureInfo.CurrentCulture);
            }
        }

        private int _popupYear; // Year shown in popup (may differ when navigating)

        public MonthYearPicker()
        {
            InitializeComponent();
            DisplayText = SelectedMonthYear.ToString("MMM yyyy", CultureInfo.CurrentCulture);
        }

        private void PickerButton_Click(object sender, RoutedEventArgs e)
        {
            PickerPopup.IsOpen = true;
        }

        private void PickerPopup_Opened(object sender, EventArgs e)
        {
            _popupYear = SelectedMonthYear.Year;
            YearText.Text = _popupYear.ToString();
        }

        private void PickerPopup_Closed(object sender, EventArgs e)
        {
            // No-op
        }

        private void YearPrev_Click(object sender, RoutedEventArgs e)
        {
            _popupYear--;
            YearText.Text = _popupYear.ToString();
        }

        private void YearNext_Click(object sender, RoutedEventArgs e)
        {
            _popupYear++;
            YearText.Text = _popupYear.ToString();
        }

        private void Month_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null && int.TryParse(btn.Tag.ToString(), out var month) && month >= 1 && month <= 12)
            {
                var value = new DateTime(_popupYear, month, 1);
                SelectedMonthYear = value;
                PickerPopup.IsOpen = false;
            }
        }
    }
}
