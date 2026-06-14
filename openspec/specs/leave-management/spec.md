## Purpose

Allow users to record and manage leave entries (full-day and half-day) from a dedicated Leaves tab, persisted locally alongside other app data.

## Requirements

### Requirement: User can manage one leave entry per date
The application SHALL allow users to add, update, and delete a single leave record per calendar date from the Leaves tab.

#### Scenario: Add full-day sick leave
- **WHEN** the user selects a date, chooses Full day duration and Sick Leave type, and saves a new entry
- **THEN** the leave is persisted for that date
- **AND** the Leaves tab calendar shows the date as having leave

#### Scenario: Add morning half-day casual leave
- **WHEN** the user selects a date, chooses Morning half day duration and Casual Leave type, and saves
- **THEN** the leave is persisted with morning half-day duration

#### Scenario: Add afternoon half-day earned leave
- **WHEN** the user selects a date, chooses Afternoon half day duration and Earned Leave type, and saves
- **THEN** the leave is persisted with afternoon half-day duration

#### Scenario: Add comp off leave
- **WHEN** the user selects a date, chooses any supported duration and Comp Off type, and saves
- **THEN** the leave is persisted with Comp Off type

#### Scenario: Update existing leave entry
- **WHEN** the user selects a date that already has leave and changes duration or type, then saves
- **THEN** the existing leave record for that date is updated in storage

#### Scenario: Delete leave entry
- **WHEN** the user selects a date with leave and deletes the entry
- **THEN** the leave record is removed from storage
- **AND** the date no longer appears as a leave date in the Leaves calendar

#### Scenario: Duplicate leave for same date is rejected
- **WHEN** the user attempts to add a new leave entry for a date that already has leave without editing the existing record
- **THEN** the application does not create a second record for that date

### Requirement: Leaves tab provides month calendar navigation
The Leaves tab SHALL display a month calendar view with identifiable leave dates and month navigation.

#### Scenario: Navigate months in Leaves tab
- **WHEN** the user changes month or year in the Leaves tab
- **THEN** the calendar reloads leave entries for the selected month

#### Scenario: Click leave date shows details
- **WHEN** the user clicks a calendar date that has leave
- **THEN** the leave details (date, duration, type) are shown in the form for update or delete

#### Scenario: Click date without leave prepares new entry
- **WHEN** the user clicks a calendar date without leave
- **THEN** the form is set to that date ready for a new leave entry

### Requirement: Leave data is persisted using app JSON storage pattern
The application SHALL persist leave entries using the existing local JSON file approach via a dedicated leave service and app path helper.

#### Scenario: Leave survives application restart
- **WHEN** the user saves a leave entry and restarts the application
- **THEN** the leave entry is still available for the saved date

#### Scenario: Leave storage includes required fields
- **WHEN** a leave entry is saved
- **THEN** the stored record includes date, leave duration, and leave type
