## Purpose

Show leave on activity month calendar tiles with themeable left-border indicators that distinguish full-day and half-day leave without disrupting existing tile content.

## Requirements

### Requirement: Activity month view shows themeable leave indicator on date tiles
The activity month view SHALL display a thick left-side border indicator on date tiles that have leave, using theme resource brushes.

#### Scenario: Full day leave shows full-height left border
- **WHEN** a date tile in the activity month view has a full-day leave entry
- **THEN** a thick left border spans the full height of the tile
- **AND** the border uses `LeaveIndicatorBrush` from application theme resources

#### Scenario: Morning half day shows top-half left border
- **WHEN** a date tile has a morning half-day leave entry
- **THEN** a thick left border is shown on the top half of the tile only

#### Scenario: Afternoon half day shows bottom-half left border
- **WHEN** a date tile has an afternoon half-day leave entry
- **THEN** a thick left border is shown on the bottom half of the tile only

#### Scenario: Date without leave shows no leave indicator
- **WHEN** a date tile has no leave entry
- **THEN** no leave left-border indicator is displayed

### Requirement: Leave indicator layers with existing month tile UI
The leave indicator SHALL NOT replace or remove existing manual-task highlight, activity bar, or date number display.

#### Scenario: Leave and manual task on same date
- **WHEN** a date has both leave and manual tasks
- **THEN** the manual-task highlight and activity bar remain visible
- **AND** the leave left-border indicator is shown alongside them

#### Scenario: Leave indicator uses shared theme resources
- **WHEN** leave indicator styles are defined
- **THEN** colors are defined in `Styles/Styles.xaml` rather than hardcoded inline values in the activity month template

### Requirement: Activity month leave data refreshes with leave changes
The activity month view SHALL reflect leave indicator changes after leave entries are added, updated, or deleted.

#### Scenario: Add leave updates activity month indicator
- **WHEN** the user adds leave for a date in the currently displayed activity month
- **THEN** the corresponding activity month tile shows the correct leave indicator without requiring an unrelated tab action

#### Scenario: Delete leave removes activity month indicator
- **WHEN** the user deletes leave for a date in the currently displayed activity month
- **THEN** the leave indicator is removed from that activity month tile
