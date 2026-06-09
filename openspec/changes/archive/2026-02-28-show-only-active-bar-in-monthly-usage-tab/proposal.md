## Why

The monthly usage tab currently displays three bars (Active, Idle, Locked) in the activity chart. This creates visual complexity and clutter. Users primarily care about active time tracked each day, making the additional bars less useful for the monthly overview. Simplifying to show only the Active bar improves visual clarity and focuses attention on the most important metric.

## What Changes

- Monthly Usage tab chart displays only the Active bar instead of Active/Idle/Locked
- Removes Idle and Locked bars from the monthly chart visualization
- Maintains all three bars in the Daily Details tab (no change to day view)
- Improves visual simplicity and data-to-ink ratio in the monthly view

## Capabilities

### New Capabilities
- `monthly-chart-active-only`: Display only Active metric in monthly usage chart while keeping all metrics in daily view

### Modified Capabilities
<!-- No existing capability requirements are changing - this is purely a UI presentation change -->

## Impact

- **Code**: ActivityChartViewModel, MonthlyActivityChartViewModel configuration, chart binding logic
- **UI**: Monthly Usage tab activity chart display configuration
- **Data**: No data model changes - only chart presentation layer
- **User Experience**: Cleaner, simpler monthly chart focused on active time
