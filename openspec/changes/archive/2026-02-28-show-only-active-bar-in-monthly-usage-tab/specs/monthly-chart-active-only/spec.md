## ADDED Requirements

### Requirement: Monthly usage chart displays only Active metric
The Monthly Usage tab activity chart SHALL display only the Active time metric. Idle and Locked bars SHALL NOT be visible in the monthly view.

#### Scenario: Monthly chart shows Active bar only
- **WHEN** the user navigates to the Monthly Usage tab
- **THEN** the activity chart displays only the Active series (Active time per day)
- **AND** the Idle and Locked bars are hidden

#### Scenario: Daily chart maintains all metrics
- **WHEN** the user switches to or stays in the Daily Details tab
- **THEN** the activity chart displays all three series: Active, Idle, Locked
- **AND** no filtering is applied to daily view

### Requirement: Chart displays accurate active time values
The monthly chart's Active bar SHALL accurately represent the total active time for each day, calculated from the tracking data, regardless of what other metrics may be filtered out.

#### Scenario: Active bar reflects correct daily active time
- **WHEN** the user views the monthly chart for a given month
- **THEN** each day's Active bar height corresponds to the actual tracked active time for that day
- **AND** the values match the daily view's Active bar for the same day

### Requirement: No data loss from filtering
Filtering the display to show only the Active metric SHALL NOT affect underlying data. All tracking data (Active, Idle, Locked) remains stored and available for other views and exports.

#### Scenario: Data integrity maintained
- **WHEN** the monthly chart filters display to Active only
- **THEN** the underlying data model still contains Active, Idle, and Locked metrics
- **AND** switching back to daily view correctly shows all three metrics
