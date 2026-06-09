## ADDED Requirements

### Requirement: Tracking does not start automatically on application launch
The application SHALL NOT start tracking automatically when the application is opened. Tracking SHALL start only when the user explicitly clicks the Start button.

#### Scenario: Application launch defaults to stopped
- **WHEN** the user launches the application
- **THEN** tracking is in Stopped state
- **AND** no tracking timers/background loops are running

#### Scenario: Start button begins tracking
- **WHEN** the user clicks Start while tracking is Stopped
- **THEN** tracking transitions to Running state
- **AND** tracking begins collecting activity data

#### Scenario: Stop button stops tracking
- **WHEN** the user clicks Stop while tracking is Running
- **THEN** tracking transitions to Stopped state
- **AND** tracking stops collecting activity data

### Requirement: Startup behavior ignores prior running state (if persisted)
The application SHALL start in Stopped state even if tracking was Running when the app was last closed.