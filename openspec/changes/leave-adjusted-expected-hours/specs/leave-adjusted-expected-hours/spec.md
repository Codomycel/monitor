## ADDED Requirements

### Requirement: Expected hours deduct leave using shared rules
The application SHALL compute leave-adjusted expected hours using shared logic with these baselines and deductions:
- Normal day expected hours: 8h
- Normal week expected hours: 40h
- Full-day leave deduction: 8h
- Half-day leave deduction (morning or afternoon): 4h
- Expected hours SHALL NOT be less than 0h

Day and week calculations SHALL use the same deduction helper so rules cannot diverge between panels.

#### Scenario: Full-day leave on selected day
- **WHEN** the selected date has full-day leave recorded
- **THEN** the day expected hours are 0h
- **AND** the week expected hours are reduced by 8h for that date (within the 40h week baseline)

#### Scenario: Morning half-day leave on selected day
- **WHEN** the selected date has morning half-day leave recorded
- **THEN** the day expected hours are 4h
- **AND** the week expected hours are reduced by 4h for that date

#### Scenario: Afternoon half-day leave on selected day
- **WHEN** the selected date has afternoon half-day leave recorded
- **THEN** the day expected hours are 4h
- **AND** the week expected hours are reduced by 4h for that date

#### Scenario: No leave on selected day
- **WHEN** the selected date has no leave recorded
- **THEN** the day expected hours are 8h
- **AND** that date contributes 0h leave deduction to the week total

#### Scenario: Multiple leave days in one week
- **WHEN** the selected week contains more than one date with leave
- **THEN** week expected hours equal 40h minus the sum of per-day deductions
- **AND** week expected hours never fall below 0h

#### Scenario: Leave deductions exceed week baseline
- **WHEN** total leave deductions in the selected week exceed 40h
- **THEN** week expected hours are 0h

### Requirement: Selected Day panel shows leave when present
The Selected Day panel SHALL display leave information when the selected date has a leave entry.

#### Scenario: Selected day with leave
- **WHEN** the user views a date that has leave in the Selected Day panel
- **THEN** leave duration and type are shown in a dedicated leave summary area
- **AND** leave-adjusted day expected hours are reflected in the day benchmark/reference context

#### Scenario: Selected day without leave
- **WHEN** the user views a date without leave in the Selected Day panel
- **THEN** the leave summary area is hidden or empty
- **AND** day expected hours remain 8h

### Requirement: Week Report shows leave summary when present
The Week Report panel SHALL display a leave summary when any day in the selected week has leave.

#### Scenario: Selected week includes leave
- **WHEN** at least one day in the selected week has leave
- **THEN** the Week Report shows a leave summary for that week
- **AND** leave-adjusted week expected hours are reflected in the weekly benchmark/reference context

#### Scenario: Selected week has no leave
- **WHEN** no day in the selected week has leave
- **THEN** the Week Report leave summary is hidden or empty
- **AND** week expected hours remain 40h

### Requirement: Week Report header format is preserved
The Week Report header SHALL use the combined week label format and SHALL NOT show a separate week-number row.

#### Scenario: Week navigation updates header
- **WHEN** the user navigates previous week, next week, current week, or selects a week via the date picker
- **THEN** the header displays `Week Report (WK{n})` with uppercase `WK` and no space before the number
- **AND** no additional standalone week-number row appears below the header

#### Scenario: Week range text unchanged
- **WHEN** the selected week changes
- **THEN** `SelectedWeekRangeText` continues to show the date range for the selected week

### Requirement: Leave changes refresh only affected day and week surfaces
When leave is added, updated, or deleted, the application SHALL refresh expected hours and leave summaries only for affected Selected Day and Week Report contexts, without unnecessarily rebuilding the full activity month grid.

#### Scenario: Leave change on selected date
- **WHEN** leave is saved or deleted for the currently selected date
- **THEN** Selected Day leave summary and expected hours update immediately
- **AND** Week Report updates if that date is in the selected week

#### Scenario: Leave change on non-selected date in selected week
- **WHEN** leave is saved or deleted for a date in the selected week that is not the selected date
- **THEN** Week Report leave summary and expected hours update
- **AND** Selected Day does not change unless its date matches the leave date

#### Scenario: Leave change outside selected week and selected day
- **WHEN** leave is saved or deleted for a date outside the selected week and not equal to the selected date
- **THEN** Selected Day and Week Report do not reload unrelated data
- **AND** the application does not call a full month grid rebuild solely for expected-hours refresh

#### Scenario: Activity month leave indicators stay consistent
- **WHEN** leave is saved or deleted for a date in the currently displayed activity month
- **THEN** the corresponding month calendar cell leave indicators are updated in place
- **AND** a full `LoadMonthlyUsage()` rebuild is not required for expected-hours-only updates
