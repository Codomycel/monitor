## ADDED Requirements

### Requirement: Horizontal Activity Bar displays time segments
The HorizontalActivityBar control SHALL display four time segments in a horizontal stacked bar format: Active, Manual Tasks, Idle, and Locked. The segments SHALL be proportional to their duration values.

#### Scenario: All segments have data
- **WHEN** Active=2h, Manual=1h, Idle=3h, Locked=1h
- **THEN** the bar shows four colored segments with widths proportional to their durations
- **AND** Active segment uses gradient color based on total active hours vs reference
- **AND** Manual segment uses amber color (#F59E0B)
- **AND** Idle segment uses light grey (#9CA3AF)
- **AND** Locked segment uses neutral grey (#6B7280)

#### Scenario: Zero total duration
- **WHEN** all durations are zero
- **THEN** the bar shows only an empty background (#E5E7EB)
- **AND** no exception is thrown

### Requirement: Total Active calculation
The system SHALL calculate Total Active as the sum of Active and Manual Tasks durations. Total Active SHALL be displayed in the tooltip but SHALL NOT be displayed as a separate segment in the bar.

#### Scenario: Active and Manual Tasks present
- **WHEN** Active=4h and Manual=2h
- **THEN** Total Active displays as "6 hours 0 minutes" in tooltip
- **AND** Active segment color is calculated based on 6 hours vs reference time
- **AND** bar shows Active and Manual as separate segments

### Requirement: Tooltip behavior
The HorizontalActivityBar SHALL display a tooltip on hover showing the breakdown of all time segments. The tooltip SHALL show Total Active, Active, Manual Tasks, Idle, and Locked durations.

#### Scenario: Hover over bar
- **WHEN** user hovers over the horizontal activity bar
- **THEN** tooltip appears showing:
  - Total Active: X hours Y minutes
  - Active: X hours Y minutes
  - Manual Tasks: X hours Y minutes
  - Idle: X hours Y minutes
  - Locked: X hours Y minutes

### Requirement: Tooltip pin on click
The tooltip SHALL support a "pin" behavior where clicking on the bar or Total Active text keeps the tooltip open. A second click on the same element SHALL close the tooltip.

#### Scenario: Click to pin tooltip
- **WHEN** user clicks on the horizontal activity bar
- **THEN** tooltip remains open (pinned)
- **WHEN** user clicks the same bar again
- **THEN** tooltip closes

#### Scenario: Click Total Active text to pin
- **WHEN** user clicks on Total Active hours text
- **THEN** tooltip remains open (pinned)
- **WHEN** user clicks the same text again
- **THEN** tooltip closes

### Requirement: No axis or labels inside bar
The HorizontalActivityBar SHALL NOT display any axis lines, axis labels, or text labels inside the bar segments.

#### Scenario: Bar renders
- **WHEN** the horizontal activity bar is displayed
- **THEN** no X or Y axis is visible
- **AND** no text labels appear inside bar segments
- **AND** only colored segments are visible

### Requirement: Reuse existing colors
The HorizontalActivityBar SHALL use the same color values as the existing vertical ActivityChart:
- Active segment: gradient from #EF4444 (low) to #10B981 (high)
- Manual segment: #F59E0B (amber)
- Idle segment: #9CA3AF (light grey)
- Locked segment: #6B7280 (neutral grey)

#### Scenario: Colors match
- **WHEN** comparing horizontal bar to vertical chart for same data
- **THEN** colors for corresponding segments match exactly
