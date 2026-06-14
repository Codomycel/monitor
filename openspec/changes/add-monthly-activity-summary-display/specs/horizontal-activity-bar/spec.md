## ADDED Requirements

### Requirement: Horizontal Activity Bar displays time segments
The HorizontalActivityBar control SHALL display four time segments in a horizontal stacked bar format: Active, Manual Tasks, Idle, and Locked. The segments SHALL be proportional to their duration values.

#### Scenario: All segments have data
- **WHEN** Active=2h, Manual=1h, Idle=3h, Locked=1h
- **THEN** the bar shows four colored segments with widths proportional to their durations
- **AND** Active segment uses gradient color based on total active hours vs reference (using existing ActivityChartViewModel color logic)
- **AND** Manual segment uses a color from the existing chart palette (orange #FBA73C from gradient - TEMPORARY, to be themed later)
- **AND** Idle segment uses light grey (#9CA3AF) from existing chart
- **AND** Locked segment uses neutral grey (#6B7280) from existing chart

#### Scenario: Zero total duration
- **WHEN** all durations are zero
- **THEN** the bar shows only an empty background (#E5E7EB)
- **AND** no exception is thrown

### Requirement: Total Active display in month view
The month view SHALL display Total Active hours text for each day directly in the calendar cell. Total Active SHALL be calculated as Active + Manual Tasks and SHALL be visible without requiring hover.

#### Scenario: Day has activity data
- **WHEN** a calendar day has Active=3h and Manual=1h
- **THEN** "4h 0m" (or similar format) displays directly in the month view cell
- **AND** the text is positioned near the horizontal activity bar

### Requirement: Total Active calculation
The system SHALL calculate Total Active as the sum of Active and Manual Tasks durations. Total Active SHALL be displayed in both the month view cell AND the tooltip. Total Active SHALL NOT be displayed as a separate segment in the bar.

#### Scenario: Active and Manual Tasks present
- **WHEN** Active=4h and Manual=2h
- **THEN** "Total Active: 6 hours 0 minutes" displays in tooltip
- **AND** "6h 0m" (or similar) displays in month view cell
- **AND** Active segment color is calculated based on 6 hours vs reference time
- **AND** bar shows Active and Manual as separate segments

### Requirement: Shared tooltip for text and bar
Both the Total Active hours text AND the horizontal activity bar SHALL share the same tooltip content. Hovering over either element SHALL show the same tooltip with the activity breakdown.

#### Scenario: Hover over Total Active text
- **WHEN** user hovers over the Total Active hours text
- **THEN** tooltip appears showing activity breakdown

#### Scenario: Hover over horizontal bar
- **WHEN** user hovers over the horizontal activity bar
- **THEN** the same tooltip appears showing:
  - Total Active: X hours Y minutes
  - Active: X hours Y minutes
  - Manual Tasks: X hours Y minutes
  - Idle: X hours Y minutes
  - Locked: X hours Y minutes

### Requirement: Tooltip pin on click
The tooltip SHALL support a "pin" behavior where clicking on either the horizontal bar OR the Total Active text keeps the tooltip open. A second click on the same element SHALL close the tooltip.

#### Scenario: Click bar to pin tooltip
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
The HorizontalActivityBar SHALL use ONLY colors already defined in the existing vertical ActivityChartViewModel:
- Active segment: gradient from #EF4444 (low) to #10B981 (high) - reuse existing GetTotalActiveColor logic
- Manual segment: #FBA73C (orange from existing gradient) - TEMPORARY color pending theme update
- Idle segment: #9CA3AF (light grey) - from existing chart
- Locked segment: #6B7280 (neutral grey) - from existing chart
- All colors SHALL be centralized in the ViewModel for easy theme changes

#### Scenario: Colors come from existing palette
- **WHEN** examining HorizontalActivityBar colors
- **THEN** all segment colors match colors already present in ActivityChartViewModel
- **AND** no new hardcoded colors are introduced
