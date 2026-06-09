## Context

The Monthly Usage tab currently displays an activity chart with three series: Active, Idle, and Locked times. The ActivityChartViewModel supports filtering which series are visible through a configuration or data-binding mechanism. The MainWindowViewModel creates a `_monthlyActivityChartViewModel` instance for this view.

Current architecture uses LiveCharts for visualization with data series that can be shown/hidden based on configuration.

## Goals / Non-Goals

**Goals:**
- Configure monthly usage chart to display only the Active series
- Maintain existing Daily Details chart with all three series (Active, Idle, Locked)
- Minimal code changes - leverage existing chart configuration mechanisms
- Preserve all data; only change visualization

**Non-Goals:**
- Changing the underlying data model or storage
- Modifying daily view chart behavior
- Adding user settings to toggle this behavior
- Changing data collection or calculation logic

## Decisions

### 1. Configure via ActivityChartViewModel visibility property
**Decision**: Use a property on ActivityChartViewModel to control which data series are visible (Active only for monthly, all three for daily)

**Rationale**: ActivityChartViewModel already manages chart display. Adding a visibility configuration is localized and doesn't require schema changes.

**Alternatives Considered**:
- Hard-code which series to show for each instance (inflexible)
- Create separate chart ViewModels for monthly vs daily (unnecessary duplication)

### 2. Initialize monthly chart with Active-only visibility
**Decision**: In MainWindowViewModel constructor, when creating `_monthlyActivityChartViewModel`, set its visibility to show only Active series

**Rationale**: Centralizes configuration at creation time rather than scattered throughout binding logic.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Daily view accidentally filters bars | Verify daily chart context (_activityChartViewModel) is not affected; explicitly initialize with all-series-visible |
| Users unfamiliar with new layout | Document in release notes that monthly view is simplified |
| Regression if data binding logic assumes all series exist | Code review to verify filtering logic handles single-series gracefully |
