## 1. Create HorizontalActivityBarViewModel

- [ ] 1.1 Create `Operon/ViewModels/HorizontalActivityBarViewModel.cs`
- [ ] 1.2 Add TimeSpan properties for ActiveDuration, ManualDuration, IdleDuration, LockedDuration
- [ ] 1.3 Add computed TotalActiveDuration property (Active + Manual)
- [ ] 1.4 Add Width properties (ActiveWidth, ManualWidth, IdleWidth, LockedWidth) for proportional sizing
- [ ] 1.5 Add Visibility boolean properties (ShowActive, ShowManual, ShowIdle, ShowLocked)
- [ ] 1.6 Add corner radius visibility properties (ShowManualNoActive, ShowOnlyActive, ShowManualEnd, etc.)
- [ ] 1.7 Add Brush properties (ActiveBrush, ManualBrush, IdleBrush, LockedBrush, EmptyBackgroundBrush)
- [ ] 1.8 Add tooltip string properties (TooltipTotalActive, TooltipActive, TooltipManual, TooltipIdle, TooltipLocked)
- [ ] 1.9 Implement SetData method to populate all durations and trigger recalculation
- [ ] 1.10 Implement color interpolation logic matching ActivityChartViewModel
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
- [ ] 5.2 Add Total Active TextBlock above the horizontal bar
- [ ] 5.3 Replace ActivityChart with HorizontalActivityBar in the template
- [ ] 5.4 Bind HorizontalActivityBar.DataContext to HorizontalBarViewModel
- [ ] 5.5 Add click event handling for tooltip pin behavior (may require behavior or code-behind)
- [ ] 5.6 Verify visibility triggers work correctly (IsFuture, HasChart, IsWeeklySummary)

## 6. Testing and Validation

- [ ] 6.1 Build project successfully with no errors
- [ ] 6.2 Verify Total Active displays correctly (Active + Manual)
- [ ] 6.3 Verify horizontal bar renders with correct segment proportions
- [ ] 6.4 Verify colors match existing vertical chart
- [ ] 6.5 Verify tooltip shows on hover
- [ ] 6.6 Verify tooltip pins on click and closes on second click
- [ ] 6.7 Verify zero duration shows empty background without crashing
- [ ] 6.8 Verify weekly summary cells still display correctly
- [ ] 6.9 Verify future dates show no chart (empty)

## 7. Documentation

- [ ] 7.1 Update any relevant README or documentation
- [ ] 7.2 Verify all acceptance criteria are met
