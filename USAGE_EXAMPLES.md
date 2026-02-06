# ActivitySummaryBarChart Usage Examples

## Week View Usage Example

```xml
<!-- Week View XAML -->
<StackPanel>
    <TextBlock Text="Week Summary" FontSize="18" FontWeight="Bold" Margin="0,0,0,16"/>
    
    <controls:ActivitySummaryBarChart 
        ChartTitle="Week Activity Summary"
        TotalActiveSeconds="{Binding WeekTotalActiveSeconds}"
        LockedSeconds="{Binding WeekLockedSeconds}"
        IdleSeconds="{Binding WeekIdleSeconds}"
        ActiveSeconds="{Binding WeekActiveSeconds}"
        ManualSeconds="{Binding WeekManualSeconds}"
        ShowTargetLine="True"
        TargetSeconds="28800"
        EnableTotalActiveThresholdColors="True"/>
        
    <!-- Other week content -->
    <DataGrid ItemsSource="{Binding WeekData}" AutoGenerateColumns="False"/>
</StackPanel>
```

## Month View Usage Example

```xml
<!-- Month View XAML -->
<StackPanel>
    <TextBlock Text="Monthly Overview" FontSize="18" FontWeight="Bold" Margin="0,0,0,16"/>
    
    <controls:ActivitySummaryBarChart 
        ChartTitle="Monthly Activity Summary"
        TotalActiveSeconds="{Binding SelectedMonthTotalActiveSeconds}"
        LockedSeconds="{Binding SelectedMonthLockedSeconds}"
        IdleSeconds="{Binding SelectedMonthIdleSeconds}"
        ActiveSeconds="{Binding SelectedMonthActiveSeconds}"
        ManualSeconds="{Binding SelectedMonthManualSeconds}"
        ShowTargetLine="True"
        TargetSeconds="28800"
        EnableTotalActiveThresholdColors="True"/>
        
    <!-- Other month content -->
    <Calendar SelectedDate="{Binding SelectedMonth}" />
    <DataGrid ItemsSource="{Binding MonthlyData}" AutoGenerateColumns="False"/>
</StackPanel>
```

## ViewModel Properties Required

Your ViewModel needs to provide these properties (all in seconds):

```csharp
// Week View ViewModel Example
public long WeekTotalActiveSeconds => _weekTotalActiveDuration.TotalSeconds;
public long WeekLockedSeconds => _weekLockedDuration.TotalSeconds;
public long WeekIdleSeconds => _weekIdleDuration.TotalSeconds;
public long WeekActiveSeconds => _weekActiveDuration.TotalSeconds;
public long WeekManualSeconds => _weekManualDuration.TotalSeconds;

// Month View ViewModel Example  
public long SelectedMonthTotalActiveSeconds => _selectedMonthTotalActiveDuration.TotalSeconds;
public long SelectedMonthLockedSeconds => _selectedMonthLockedDuration.TotalSeconds;
public long SelectedMonthIdleSeconds => _selectedMonthIdleDuration.TotalSeconds;
public long SelectedMonthActiveSeconds => _selectedMonthActiveDuration.TotalSeconds;
public long SelectedMonthManualSeconds => _selectedMonthManualDuration.TotalSeconds;
```

## Custom Target Line Example

```xml
<!-- Custom target line (e.g., 6-hour goal) -->
<controls:ActivitySummaryBarChart 
    ...
    ShowTargetLine="True"
    TargetSeconds="21600"  <!-- 6 hours = 21600 seconds -->
    TargetLabel="6h"
    ...
/>
```

## Disable Threshold Colors Example

```xml
<!-- Disable conditional coloring (all bars blue) -->
<controls:ActivitySummaryBarChart 
    ...
    EnableTotalActiveThresholdColors="False"
    ...
/>
```

## Regression Test Example

### Simple Test View
```xml
<!-- Test UserControl with sample data -->
<StackPanel>
    <TextBlock Text="Activity Summary Test" FontSize="18" FontWeight="Bold" Margin="0,0,0,16"/>
    
    <controls:ActivitySummaryBarChart 
        TotalActiveSeconds="18000"    <!-- 5 hours -->
        LockedSeconds="3600"        <!-- 1 hour -->
        IdleSeconds="1800"          <!-- 30 minutes -->
        ActiveSeconds="14400"       <!-- 4 hours -->
        ManualSeconds="3600"        <!-- 1 hour -->
        ShowTargetLine="True"
        TargetSeconds="28800"       <!-- 8 hours -->
        EnableTotalActiveThresholdColors="True"/>
        
    <!-- Test data update button -->
    <Button Content="Test Update (10h Total)" Click="TestUpdate_Click" Margin="0,16,0,0"/>
</StackPanel>
```

### Test ViewModel
```csharp
public class TestViewModel : INotifyPropertyChanged
{
    private long _totalActiveSeconds = 18000; // 5h
    private long _lockedSeconds = 3600;       // 1h
    private long _idleSeconds = 1800;         // 30m
    private long _activeSeconds = 14400;      // 4h
    private long _manualSeconds = 3600;       // 1h

    public long TotalActiveSeconds
    {
        get => _totalActiveSeconds;
        set { _totalActiveSeconds = value; OnPropertyChanged(nameof(TotalActiveSeconds)); }
    }
    
    // Other properties similar...
    
    public void TestUpdate_Click()
    {
        // Update to 10 hours total
        TotalActiveSeconds = 36000; // 10h
        ActiveSeconds = 28800;     // 8h
        ManualSeconds = 7200;      // 2h
    }
}
```

### Expected Behavior
1. **Initial State**: 5h Total Active bar below 8h line
2. **After Update**: 10h Total Active bar crosses 8h line, axis expands
3. **Tooltip**: Shows detailed breakdown with proper typography
4. **Colors**: Locked (Crimson Red), Idle (Teal), Total Active (threshold-based)

## Key Features

- **3 Fixed Categories**: Total Active, Locked, Idle (always in this order)
- **Green Target Line**: 8h default, customizable
- **Conditional Coloring**: Total Active bar changes color based on thresholds
- **Enhanced Tooltip**: Big Total Active + detailed breakdown
- **Accurate Scaling**: Y-axis max includes target line + padding
- **Clean Labels**: Bar labels show "Hh" or "H.MMh" format
- **Reusable**: Drop-in component with bindable properties
- **Binding Fixed**: Uses RelativeSource to UserControl for all internal bindings
