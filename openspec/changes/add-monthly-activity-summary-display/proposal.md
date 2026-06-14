## Why

The monthly calendar view currently shows activity data through a vertical chart control that takes significant vertical space and includes axes. Users need a more compact, at-a-glance view of their daily activity with the Total Active hours clearly displayed alongside a horizontal stacked bar showing the breakdown of time segments (Active, Manual Tasks, Idle, Locked).

## What Changes

- Add a reusable `HorizontalActivityBar` WPF user control that displays activity segments as a horizontal stacked bar
- Display Total Active hours text in each monthly calendar day cell (Total Active = Active + Manual Tasks)
- Integrate the horizontal activity bar into the monthly view grid alongside the Total Active text
- Implement interactive tooltip behavior on both the Total Active text and the horizontal bar:
  - Hover shows tooltip with activity breakdown
  - Click pins tooltip open
  - Second click closes tooltip
- Reuse existing color scheme from the vertical activity chart (no new colors)
- Keep existing `ActivityChartViewModel`/`ChartViewModel` intact for other views

## Capabilities

### New Capabilities
- `horizontal-activity-bar`: A reusable WPF component for displaying activity time segments in a compact horizontal stacked bar format with interactive tooltip support

### Modified Capabilities
- None (existing specs remain unchanged; this is a UI enhancement using existing data structures)

## Impact

- **UIA Main Window**: Monthly calendar grid item template will be updated to show Total Active text and HorizontalActivityBar
- **New Files**:
  - `Operon/Controls/HorizontalActivityBar.xaml` - XAML definition
  - `Operon/Controls/HorizontalActivityBar.xaml.cs` - Code-behind
  - `Operon/ViewModels/HorizontalActivityBarViewModel.cs` - ViewModel for the control
- **Modified Files**:
  - `Operon/ViewModels/MainWindowViewModel.cs` - Add HorizontalBarViewModel to MonthlyDayItem and populate data
  - `Operon/Views/UiAMainWindow.xaml` - Update calendar item template
- **Dependencies**: None (uses existing MVVM patterns and LiveCharts colors)
