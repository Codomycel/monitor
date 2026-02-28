## 1. Analyze Chart Configuration

- [x] 1.1 Review ActivityChartViewModel to understand how data series visibility works
- [x] 1.2 Investigate LiveCharts configuration for showing/hiding series
- [x] 1.3 Review how _monthlyActivityChartViewModel is created in MainWindowViewModel

## 2. Implement Series Visibility Control

- [x] 2.1 Add visibility configuration property/method to ActivityChartViewModel
- [x] 2.2 Implement logic to filter data series based on visibility flags
- [x] 2.3 Update chart binding to respect visibility configuration

## 2. Refine Series Visibility Implementation

- [x] 2.1 Ensure `ShowTotalActivityOnly` triggers chart refresh
  - [x] 2.1.1 In `ShowTotalActivityOnly` setter, call `UpdateChart()` (or rebuild series + update)
- [x] 2.2 Monthly mode must remove `Idle`/`Locked` series objects (not just set values to `0`)
  - [x] 2.2.1 Monthly `ChartSeries.Count` must be **1**
  - [x] 2.2.2 Monthly `XAxisLabels` must be `["Total Active"]`
- [x] 2.3 If gray line still appears: disable X-axis separator for monthly mode
  - [x] 2.3.1 Add a VM flag `ShowXAxisSeparator`
  - [x] 2.3.2 Bind `Separator.IsEnabled` to `ShowXAxisSeparator` in XAML

## 3. Configure Monthly Chart

- [x] 3.1 Update MainWindowViewModel to configure _monthlyActivityChartViewModel with Active-only visibility
- [x] 3.2 Verify _activityChartViewModel (daily chart) remains configured with all series visible
- [x] 3.3 Ensure initial data population respects the visibility configuration

## 4. Testing and Validation

- [x] 4.1 Test monthly view displays only Active bar
- [x] 4.2 Test daily view displays all three bars (Active, Idle, Locked)
- [x] 4.3 Test switching between monthly and daily views works correctly
- [x] 4.4 Test chart updates correctly when underlying data changes
- [x] 4.5 Regression: verify existing charts and summaries still work correctly
- [x] 4.6 Verify data integrity - confirm no data is lost from filtering
