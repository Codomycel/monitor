## MODIFIED Requirements

### Requirement: Application prevents multiple instances from running
The application SHALL implement a single instance enforcement mechanism that prevents more than one instance of the portable EXE from running simultaneously. This enforcement SHALL work regardless of whether the application was started automatically or manually. When a second instance is launched, it SHALL be gracefully terminated or blocked.

#### Scenario: First instance launches successfully when started manually
- **WHEN** the application is launched manually for the first time by the user
- **THEN** the main application window displays normally and the instance lock is acquired

#### Scenario: Second instance attempts to launch while first is running
- **WHEN** a second instance of the application EXE is launched (either manually or through any mechanism) while the first instance is running
- **THEN** the second instance detects the existing instance and exits gracefully without displaying an error dialog

### Requirement: Existing instance window is brought to foreground
The application SHALL bring the already-running instance window to the foreground and focus when a second instance attempts to launch, regardless of how either instance was launched.

#### Scenario: Second manually-launched instance activates first instance window
- **WHEN** a second instance is manually launched while the first instance is running
- **THEN** the window of the first instance is brought to the foreground and given focus before the second instance exits

#### Scenario: Instance is maintained if user launches application multiple times
- **WHEN** a user attempts to launch the application multiple times in quick succession
- **THEN** only the first instance continues running and subsequent launch attempts bring that window to focus

### Requirement: Instance detection uses robust mechanism
The application SHALL use a robust mechanism (such as a named mutex, named pipe, or global lock) to detect existing instances that is resistant to crashes and system restarts. This mechanism SHALL NOT depend on automatic startup behavior.

#### Scenario: Instance lock persists while application is running
- **WHEN** the application is running after manual launch
- **THEN** the instance lock is held and prevents other instances from starting

#### Scenario: Instance lock is released on application exit
- **WHEN** the application exits normally or crashes
- **THEN** the instance lock is released and a new instance can be launched
