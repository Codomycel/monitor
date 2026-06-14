## ADDED Requirements

### Requirement: Hover tooltip displays activity breakdown
The HorizontalActivityBar control SHALL display a tooltip on hover showing the breakdown of all time segments.

#### Scenario: Hover over bar
- **WHEN** user hovers over the horizontal activity bar
- **THEN** tooltip appears showing:
  - Total Active: X hours Y minutes
  - Active: X hours Y minutes
  - Manual Tasks: X hours Y minutes
  - Idle: X hours Y minutes
  - Locked: X hours Y minutes

### Requirement: Click-pinned tooltip shows same data as hover
The click-pinned tooltip SHALL display the exact same data as the hover tooltip.

#### Scenario: Click to pin tooltip
- **WHEN** user clicks on the horizontal activity bar
- **THEN** the tooltip pins/opens and shows all activity data
- **AND** the content matches the hover tooltip exactly
- **AND** no empty or null values are shown

#### Scenario: Click again to close
- **WHEN** user clicks the same element again while tooltip is pinned
- **THEN** the pinned tooltip closes

#### Scenario: Pinned tooltip has correct data
- **GIVEN** hover tooltip shows "Total Active: 7 hours 30 minutes"
- **WHEN** user clicks to pin the tooltip
- **THEN** pinned tooltip shows "Total Active: 7 hours 30 minutes"
- **AND** all breakdown values match the hover state

### Requirement: Total Active text displays correctly
The Total Active hours text SHALL display in the format `HHh MMm` (e.g., "06h 00m").

#### Scenario: Text format
- **WHEN** Total Active is 6 hours 30 minutes
- **THEN** the text displays as "06h 30m"
