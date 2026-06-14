## 1. Create HorizontalActivityBarViewModel

- [ ] 1.1 Create `Operon/ViewModels/HorizontalActivityBarViewModel.cs`
- [ ] 1.2 Add TimeSpan properties for ActiveDuration, ManualDuration, IdleDuration, LockedDuration
- [ ] 1.3 Add computed TotalActiveDuration property (Active + Manual)
- [ ] 1.4 Add TotalActiveText property for display in month view (e.g., "6h 30m" format)
- [ ] 1.5 Add Width properties (ActiveWidth, ManualWidth, IdleWidth, LockedWidth) for proportional sizing
- [ ] 1.6 Add Visibility boolean properties (ShowActive, ShowManual, ShowIdle, ShowLocked)
- [ ] 1.7 Add corner radius visibility properties (ShowManualNoActive, ShowOnlyActive, ShowManualEnd, etc.)
- [ ] 1.8 Add centralized Brush properties - all colors defined here for easy theming:
  - ActiveBrush: uses existing gradient logic (call or duplicate ActivityChartViewModel.GetTotalActiveColor)
  - ManualBrush: use #FBA73C from existing gradient (TEMPORARY - mark in comments for theme update)
  - IdleBrush: use #9CA3AF from existing chart
  - LockedBrush: use #6B7280 from existing chart
  - EmptyBackgroundBrush: use #E5E7EB
- [ ] 1.9 Add tooltip string properties (TooltipTotalActive, TooltipActive, TooltipManual, TooltipIdle, TooltipLocked)
- [ ] 1.10 Implement SetData method to populate all durations and trigger recalculation
- [ ] 1.11 Add INotifyPropertyChanged implementation

## 2. Create HorizontalActivityBar XAML Control

- [ ] 2.1 Create `Operon/Controls/HorizontalActivityBar.xaml`
- [ ] 2.2 Define Grid layout with 4 columns for segments
- [ ] 2.3 Add Border elements for each segment with appropriate bindings
- [ ] 2.4 Add empty state background for zero total duration
- [ ] 2.5 Add tooltip definition with formatted content
- [ ] 2.6 Add BooleanToVisibilityConverter resource
- [ ] 2.7 Create `Operon/Controls/HorizontalActivityBar.xaml.cs` code-behind
- [ ] 2.8 Use fully qualified type names to avoid namespace conflicts

## 3. Update MonthlyDayItem Model

- [ ] 3.1 Add `HorizontalActivityBarViewModel? HorizontalBarViewModel` property to MonthlyDayItem class
- [ ] 3.2 Keep existing ChartViewModel property intact

## 4. Update MainWindowViewModel

- [ ] 4.1 In BuildMonthlyCalendar method, create HorizontalActivityBarViewModel alongside ChartViewModel
- [ ] 4.2 Set ReferenceTime = TimeSpan.FromHours(8) on HorizontalBarViewModel
- [ ] 4.3 Assign HorizontalBarViewModel to dayItem.HorizontalBarViewModel
- [ ] 4.4 Set data on HorizontalBarViewModel using SetData(activeTime, manualTime, idleTime, lockedTime)

## 5. Update UiAMainWindow.xaml

- [ ] 5.1 Update calendar ItemsControl ItemTemplate DataTemplate
- [ ] 5.2 Add Total Active TextBlock above the horizontal bar, bound to TotalActiveText property
- [ ] 5.3 Replace ActivityChart with HorizontalActivityBar in the template
- [ ] 5.4 Bind HorizontalActivityBar.DataContext to HorizontalBarViewModel
- [ ] 5.5 Create shared ToolTip resource in the DataTemplate for both text and bar
- [ ] 5.6 Bind ToolTip on Total Active TextBlock to the shared tooltip
- [ ] 5.7 Bind ToolTip on HorizontalActivityBar to the same shared tooltip
- [ ] 5.8 Add click event handling for tooltip pin behavior (toggle IsOpen on click)
- [ ] 5.9 Verify visibility triggers work correctly (IsFuture, HasChart, IsWeeklySummary)

## 6. Testing and Validation

- [ ] 6.1 Build project successfully with no errors
- [ ] 6.2 Verify Total Active displays correctly (Active + Manual)
- [ ] 6.3 Verify Total Active text appears directly in month view cell (not just tooltip)
- [ ] 6.4 Verify horizontal bar renders with correct segment proportions
- [ ] 6.5 Verify colors match existing vertical chart (no new colors introduced)
- [ ] 6.6 Verify Manual segment uses #FBA73C (existing gradient color, not #F59E0B)
- [ ] 6.7 Verify tooltip shows on hover over both Total Active text AND horizontal bar
- [ ] 6.8 Verify tooltip pins on click and closes on second click (both elements)
- [ ] 6.9 Verify zero duration shows empty background without crashing
- [ ] 6.10 Verify weekly summary cells still display correctly
- [ ] 6.11 Verify future dates show no chart (empty)

## 7. Documentation

- [ ] 7.1 Update any relevant README or documentation
- [ ] 7.2 Verify all acceptance criteria are met
