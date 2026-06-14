## 1. Update HorizontalActivityBarViewModel

- [x] 1.1 Create `Operon/ViewModels/HorizontalActivityBarViewModel.cs`
- [x] 1.2 Add TimeSpan properties for ActiveDuration, ManualDuration, IdleDuration, LockedDuration
- [x] 1.3 Add computed TotalActiveDuration property (Active + Manual)
- [x] 1.4 **FIX:** Update TotalActiveText property to use HHh MMm format (e.g., "06h 00m")
- [x] 1.5 **FIX:** Update Width calculation to use 8-hour reference instead of total duration
- [x] 1.6 Add Visibility boolean properties (ShowActive, ShowManual, ShowIdle, ShowLocked)
- [x] 1.7 Add corner radius visibility properties (ShowManualNoActive, ShowOnlyActive, ShowManualEnd, etc.)
- [x] 1.8 Add centralized Brush properties - all colors defined here for easy theming:
  - ActiveBrush: uses existing gradient logic (call or duplicate ActivityChartViewModel.GetTotalActiveColor)
  - ManualBrush: use #FBA73C from existing gradient (TEMPORARY - mark in comments for theme update)
  - IdleBrush: use #9CA3AF from existing chart
  - LockedBrush: use #6B7280 from existing chart
  - EmptyBackgroundBrush: use #E5E7EB
- [x] 1.9 Add tooltip string properties (TooltipTotalActive, TooltipActive, TooltipManual, TooltipIdle, TooltipLocked)
- [x] 1.10 Implement SetData method to populate all durations and trigger recalculation
- [x] 1.11 Add INotifyPropertyChanged implementation

## 2. Create HorizontalActivityBar XAML Control

- [x] 2.1 Create `Operon/Controls/HorizontalActivityBar.xaml`
- [x] 2.2 Define Grid layout with 4 columns for segments
- [x] 2.3 Add Border elements for each segment with appropriate bindings
- [x] 2.4 Add empty state background for zero total duration
- [x] 2.5 Add tooltip definition with formatted content
- [x] 2.6 Add BooleanToVisibilityConverter resource
- [x] 2.7 Create `Operon/Controls/HorizontalActivityBar.xaml.cs` code-behind
- [x] 2.8 Use fully qualified type names to avoid namespace conflicts

## 3. Update MonthlyDayItem Model

- [x] 3.1 Add `HorizontalActivityBarViewModel? HorizontalBarViewModel` property to MonthlyDayItem class
- [x] 3.2 Keep existing ChartViewModel property intact

## 4. Update MainWindowViewModel

- [x] 4.1 In BuildMonthlyCalendar method, create HorizontalActivityBarViewModel alongside ChartViewModel
- [x] 4.2 Set ReferenceTime = TimeSpan.FromHours(8) on HorizontalBarViewModel
- [x] 4.3 Assign HorizontalBarViewModel to dayItem.HorizontalBarViewModel
- [x] 4.4 Set data on HorizontalBarViewModel using SetData(activeTime, manualTime, idleTime, lockedTime)

## 5. Update UiAMainWindow.xaml

- [x] 5.1 **FIX:** Update DataTemplate for centered vertical layout
- [x] 5.2 **FIX:** Update Total Active TextBlock - larger font (14-16pt), bold, centered, HH:MM format
- [x] 5.3 **FIX:** Position horizontal bar below text as compact indicator (not full tile)
- [x] 5.4 Replace ActivityChart with HorizontalActivityBar in the template
- [x] 5.5 Bind HorizontalActivityBar.DataContext to HorizontalBarViewModel
- [x] 5.6 **FIX:** Fix shared tooltip data binding - ensure DataContext flows correctly
- [x] 5.7 **FIX:** Verify hover tooltip shows correct data from HorizontalBarViewModel
- [x] 5.8 **FIX:** Verify click-pinned tooltip shows same data as hover (not empty)
- [x] 5.9 Add click event handling for tooltip pin behavior (toggle IsOpen on click)
- [x] 5.10 Verify visibility triggers work correctly (IsFuture, HasChart, IsWeeklySummary)

## 6. Testing and Validation

- [x] 6.1 Build project successfully with no errors
- [x] 6.2 Verify Total Active displays in HHh MMm format (e.g., "06h 00m")
- [x] 6.3 Verify Total Active is visually larger and centered in tile
- [x] 6.4 Verify horizontal bar is compact and does not occupy full tile
- [x] 6.5 Verify 8 hours is treated as full bar reference length
- [x] 6.6 Verify less than 8 hours shows proportional bar within reference
- [x] 6.7 Verify more than 8 hours is visually understandable without breaking layout
- [x] 6.8 Verify colors match existing vertical chart (no new colors introduced)
- [x] 6.9 Verify Manual segment uses #FBA73C (existing gradient color, not #F59E0B)
- [x] 6.10 Verify hover tooltip shows correct data
- [x] 6.11 Verify click-pinned tooltip shows SAME correct data (not empty)
- [x] 6.12 Verify tooltip pins on click and closes on second click (both elements)
- [x] 6.13 Verify zero duration shows empty background without crashing
- [x] 6.14 Verify weekly summary cells still display correctly
- [x] 6.15 Verify future dates show no chart (empty)

## 7. Documentation

- [x] 7.1 Update any relevant README or documentation
- [x] 7.2 Verify all acceptance criteria are met
