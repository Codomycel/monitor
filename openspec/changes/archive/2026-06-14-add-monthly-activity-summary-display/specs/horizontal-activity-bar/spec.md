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
The month view SHALL display Total Active hours text for each day directly in the calendar cell. Total Active SHALL be calculated as Active + Manual Tasks and SHALL be visible without requiring hover. The text SHALL be formatted as `HHh MMm`, visually larger, and centered both horizontally and vertically within the tile.

#### Scenario: Day has activity data
- **WHEN** a calendar day has Active=3h and Manual=1h
- **THEN** "04h 00m" (HHh MMm format) displays directly in the month view cell
- **AND** the text is visually larger (e.g., FontSize 14-16, FontWeight Bold)
- **AND** the text is centered horizontally and vertically within the tile
- **AND** the horizontal bar is positioned below the text as a compact indicator

### Requirement: Total Active calculation
The system SHALL calculate Total Active as the sum of Active and Manual Tasks durations. Total Active SHALL be displayed in both the month view cell AND the tooltip. Total Active SHALL NOT be displayed as a separate segment in the bar.

#### Scenario: Active and Manual Tasks present
- **WHEN** Active=4h and Manual=2h
- **THEN** "Total Active: 6 hours 0 minutes" displays in tooltip
- **AND** "06h 00m" (HHh MMm format) displays in month view cell
- **AND** Active segment color is calculated based on 6 hours vs reference time
- **AND** bar shows Active and Manual as separate segments

### Requirement: 8-hour reference bar length
The horizontal bar SHALL use 8 hours as the visual reference/full bar length. Segments SHALL be sized proportionally within this 8-hour reference. If total tracked time exceeds 8 hours, the bar SHALL still be visually understandable without breaking the tile layout.

#### Scenario: Less than 8 hours tracked
- **WHEN** total tracked time is 6 hours
- **THEN** the filled bar occupies 75% of the reference width (6/8)
- **AND** segments are proportional within the filled portion

#### Scenario: Exactly 8 hours tracked
- **WHEN** total tracked time is 8 hours
- **THEN** the filled bar occupies the full reference width (100%)

#### Scenario: More than 8 hours tracked
- **WHEN** total tracked time is 10 hours
- **THEN** the filled bar extends beyond the reference or uses a scaled representation
- **AND** the layout remains visually understandable
- **AND** the tile does not break or overflow

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

### Requirement: Tooltip pin on click with correct data
The tooltip SHALL support a "pin" behavior where clicking on either the horizontal bar OR the Total Active text keeps the tooltip open with the SAME data as the hover tooltip. A second click on the same element SHALL close the tooltip. The pinned tooltip SHALL NOT be empty.

#### Scenario: Click bar to pin tooltip with data
- **WHEN** user clicks on the horizontal activity bar
- **THEN** tooltip remains open (pinned) showing the correct activity breakdown data
- **AND** the tooltip content matches the hover tooltip exactly
- **WHEN** user clicks the same bar again
- **THEN** tooltip closes

#### Scenario: Click Total Active text to pin with data
- **WHEN** user clicks on Total Active hours text
- **THEN** tooltip remains open (pinned) showing the correct activity breakdown data
- **AND** the tooltip content matches the hover tooltip exactly
- **WHEN** user clicks the same text again
- **THEN** tooltip closes

#### Scenario: Pinned tooltip never empty
- **WHEN** user clicks to pin the tooltip
- **THEN** the tooltip displays all activity data (Total Active, Active, Manual, Idle, Locked)
- **AND** no empty or null content is shown

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
