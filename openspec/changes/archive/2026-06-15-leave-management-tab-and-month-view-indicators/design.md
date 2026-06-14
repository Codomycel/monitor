## Context

The Operon WPF app uses `MainWindowViewModel` with tab navigation via `SelectedTabIndex` / `MainWindowTab` enum. Monthly calendar data is built in `LoadMonthlyUsage()` as `MonthlyDayItem` objects bound to a `UniformGrid` in the Monthly Usage tab. Manual tasks use per-date JSON files via `ManualTaskService` and `AppPaths.GetManualTasksPath()`.

Leave management should follow the same patterns: MVVM bindings, `RelayCommand`, theme resources in `Styles.xaml`, and JSON persistence — without touching activity tracking or manual task logic.

## Goals / Non-Goals

**Goals:**

- One leave record per date with duration and type
- Leaves tab with month calendar, form, and CRUD commands
- Activity month view left-border indicators for full/morning/afternoon leave
- Themeable indicator brushes in `Styles.xaml`
- Reuse existing calendar/month navigation patterns

**Non-Goals:**

- Multiple leave records per date
- Leave approval workflows or balances
- Changes to manual tasks, activity tracking, or unrelated tabs
- Automated test files
- New third-party packages

## Decisions

### 1. Data model — one JSON index file per month

**Decision:** Store leave entries in `leaves-YYYY-MM.json` containing a list of `LeaveEntry` objects keyed by date (enforced in service/VM).

**Rationale:** Matches app-local JSON pattern; month file aligns with calendar navigation and avoids hundreds of per-day files.

**Alternatives considered:**
- Per-date file like manual tasks — rejected; leave is at most one entry per date and month queries are common
- SQLite — rejected; no new dependencies, inconsistent with current storage

### 2. Model shape

```csharp
LeaveEntry { Guid Id, DateTime Date, LeaveDuration Duration, LeaveType Type }
LeaveDuration: FullDay | MorningHalf | AfternoonHalf
LeaveType: SickLeave | CasualLeave | EarnedLeave | CompOff
```

**Decision:** Single entry per date; save rejects duplicate date unless updating same `Id`.

### 3. Service layer — `LeaveService`

**Decision:** `LoadMonth(year, month)`, `SaveMonth(year, month, entries)`, `GetForDate(date)` — mirrors `ManualTaskService` + `JsonFile` utilities.

**Path:** `AppPaths.GetLeavesPath(year, month)` → `leaves-2026-06.json`

### 4. Leaves tab placement and enum

**Decision:** Insert Leaves tab after Manual Tasks in `UiAMainWindow.xaml`. Extend `MainWindowTab`:

```
MonthlyUsage = 0, ApplicationUsage = 1, ManualTasks = 2, Leaves = 3,
WeeklyOverview = 4, LastCrash = 5, Settings = 6
```

Update any hardcoded tab indices accordingly.

### 5. Leaves tab UI layout

**Decision:** Reuse Monthly Usage calendar structure:
- Header: month/year picker + prev/next navigation
- Body: `UniformGrid` calendar with leave-highlighted day cells
- Detail panel: date picker, duration combo, type combo, Add/Save/Delete/Cancel buttons
- Clicking a calendar day with leave loads entry into form for edit

### 6. Activity month leave indicator — left border overlay

**Decision:** Add a narrow `Border` or two stacked `Border` elements on the left edge inside the existing `CalendarDayCellStyle` template (overlay in `DataTemplate`, not replacing background).

| Duration | Visual |
|----------|--------|
| FullDay | Single `Border` Width=4, full height, `LeaveIndicatorBrush` |
| MorningHalf | Top half `Border` with `Height="50%"` VerticalAlignment=Top |
| AfternoonHalf | Bottom half `Border` with `Height="50%"` VerticalAlignment=Bottom |

Bind visibility/segment to new `MonthlyDayItem.LeaveDuration` (nullable none = hidden).

**Rationale:** Layers cleanly with manual-task background highlight and activity bar; does not replace existing tile chrome.

### 7. Populate leave data on activity month load

**Decision:** In `LoadMonthlyUsage()`, call `LeaveService` for the displayed month and set `MonthlyDayItem.LeaveDuration` / `LeaveType` per day. Refresh on leave CRUD via shared reload method.

### 8. Theme resources

**Decision:** Add to `Styles/Styles.xaml`:
- `LeaveIndicatorBrush` (primary leave color, e.g. amber/orange family)
- Optional `LeaveIndicatorMorningBrush` / `LeaveIndicatorAfternoonBrush` if type-specific coloring is needed later; start with one brush for all durations

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Tab index shift breaks navigation | Use `MainWindowTab` enum everywhere; audit `SelectedTabIndex` literals |
| Leave border clashes with manual-task highlight | Position border on left edge only; keep highlight as background |
| Half-day border rendering on small tiles | Use proportional heights (50%) inside grid row |
| Stale activity month after leave edit | Call `LoadMonthlyUsage()` after leave persist when selected month matches |

## Migration Plan

No data migration — greenfield feature. New JSON files created on first leave save.

## Open Questions

None — requirements are fully specified by the user.
