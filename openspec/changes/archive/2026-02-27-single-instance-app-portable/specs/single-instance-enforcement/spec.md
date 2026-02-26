## ADDED Requirements

### Requirement: Application prevents multiple instances from running
The application SHALL implement a single instance enforcement mechanism that prevents more than one instance of the portable EXE from running simultaneously. When a second instance is launched, it SHALL be gracefully terminated or blocked.

#### Scenario: First instance launches successfully
- **WHEN** the application is launched for the first time
- **THEN** the main application window displays normally and the instance lock is acquired

#### Scenario: Second instance attempts to launch
- **WHEN** a second instance of the application EXE is launched while the first instance is running
- **THEN** the second instance detects the existing instance and exits gracefully without displaying an error dialog

### Requirement: Existing instance window is brought to foreground
The application SHALL bring the already-running instance window to the foreground and focus when a second instance attempts to launch.

#### Scenario: Second instance activates first instance window
- **WHEN** a second instance detects an existing running instance
- **THEN** the window of the first instance is brought to the foreground and given focus before the second instance exits

### Requirement: Instance detection uses robust mechanism
The application SHALL use a robust mechanism (such as a named mutex, named pipe, or global lock) to detect existing instances that is resistant to crashes and system restarts.

#### Scenario: Instance lock persists across application running state
- **WHEN** the application is running
- **THEN** the instance lock is held and prevents other instances from starting

#### Scenario: Instance lock is released on application exit
- **WHEN** the application exits normally or crashes
- **THEN** the instance lock is released and a new instance can be launched
