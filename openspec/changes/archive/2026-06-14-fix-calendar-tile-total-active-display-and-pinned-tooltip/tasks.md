## 1. Diagnose Current State

- [x] 1.1 Inspect UiAMainWindow.xaml calendar tile DataTemplate
- [x] 1.2 Verify TotalActiveText binding path and DataContext
- [x] 1.3 Check HorizontalActivityBarViewModel TotalActiveText property implementation
- [x] 1.4 Test hover tooltip to confirm it works
- [x] 1.5 Test click-pinned tooltip to confirm it's empty

## 2. Fix Total Active Text Display

- [x] 2.1 Update TotalActiveText property to use `HHh MMm` format (e.g., "06h 00m")
- [x] 2.2 Fix XAML binding to properly display TotalActiveText from HorizontalBarViewModel
- [x] 2.3 Ensure text is centered horizontally and vertically in tile
- [x] 2.4 Set text styling: FontSize 14-16, FontWeight Bold, Foreground dark color

## 3. Fix Horizontal Bar Position and Size

- [x] 3.1 Position bar below the Total Active text
- [x] 3.2 Set compact bar dimensions: Height 6-8px, fixed Width ~60px
- [x] 3.3 Configure bar to fill left-to-right using 8h as 100% reference
- [x] 3.4 Bar represents Total Active only (single color based on gradient)

## 4. Fix Tooltip Data Binding

- [x] 4.1 Move ToolTip definition inline on Total Active TextBlock (not as shared resource)
- [x] 4.2 Move ToolTip definition inline on HorizontalActivityBar
- [x] 4.3 Bind ToolTip content directly to HorizontalBarViewModel properties
- [x] 4.4 Ensure hover tooltip shows correct data
- [x] 4.5 Ensure click-pinned tooltip shows same data as hover
- [x] 4.6 Verify second click closes pinned tooltip

## 5. Verify Tooltip Content

- [ ] 5.1 Verify tooltip shows: Total Active, Active, Manual Tasks, Idle, Locked
- [ ] 5.2 Verify all time values are formatted correctly
- [ ] 5.3 Verify tooltip styling matches existing design

## 6. Testing and Validation

- [x] 6.1 Build project with no errors (compilation succeeds, file locked by running process)
- [ ] 6.2 Tile shows Total Active in `HHh MMm` format (e.g., "06h 00m")
- [ ] 6.3 Text is centered in tile
- [ ] 6.4 Bar appears below text with correct 8h reference scaling
- [ ] 6.5 Hover tooltip has correct data
- [ ] 6.6 Click-pinned tooltip has same data (not empty)
- [ ] 6.7 Second click closes pinned tooltip
- [ ] 6.8 No new colors introduced
