## ADDED Requirements

### Requirement: Month tile Total Active text displays tooltip
The calendar/month view tile SHALL display a tooltip on the Total Active hours text showing all five activity values: Total Active hrs, Active hrs, Manual hrs, Idle hrs, Locked hrs. The horizontal total-active bar tooltip is already working correctly.

#### Scenario: Hover over Total Active text in month tile
- **WHEN** user hovers over the Total Active hours text in the month tile
- **THEN** tooltip appears showing:
  - Total Active: X hours Y minutes
  - Active: X hours Y minutes
  - Manual: X hours Y minutes
  - Idle: X hours Y minutes
  - Locked: X hours Y minutes
- **AND** the tooltip reuses the same data/content as the working horizontal bar tooltip

#### Scenario: Tooltip content is not empty
- **GIVEN** the month tile has activity data
- **WHEN** user hovers over the Total Active text
- **THEN** tooltip displays all five values with correct data
- **AND** no empty or null values are shown

### Requirement: Month tile Total Active text click toggles/pins tooltip
Clicking on the Total Active hours text in the month tile SHALL toggle the tooltip pinned state.

#### Scenario: Click to pin tooltip on text
- **WHEN** user clicks on Total Active hours text in the month tile
- **THEN** the tooltip pins/opens and stays visible
- **AND** the content matches the hover tooltip

#### Scenario: Second click on Total Active text closes tooltip
- **GIVEN** the tooltip is pinned open on Total Active text
- **WHEN** user clicks the Total Active text again
- **THEN** the pinned tooltip closes

#### Scenario: Pinned tooltip on Total Active text has same data as hover
- **GIVEN** hover tooltip on Total Active text shows "Total Active: 6 hours 30 minutes"
- **WHEN** user clicks to pin the tooltip
- **THEN** pinned tooltip shows "Total Active: 6 hours 30 minutes"
- **AND** all five values match the hover state exactly

### Requirement: Total Active text reuses horizontal bar tooltip data
The Total Active hours text tooltip SHALL reuse the same data source as the working horizontal bar tooltip.

#### Scenario: Data consistency with horizontal bar
- **WHEN** user hovers over Total Active text
- **THEN** tooltip shows same content as hovering over horizontal bar
- **AND** both tooltips share the same HorizontalBarViewModel data source

### Requirement: Tooltip limited to month tile Total Active text
The tooltip behavior for Total Active text SHALL only apply to the calendar/month view tile. The horizontal bar tooltip behavior is already working and SHALL NOT be modified. Other views SHALL NOT be affected.

#### Scenario: Other views unaffected
- **GIVEN** user is in Weekly or Daily view
- **WHEN** user hovers over activity elements
- **THEN** no tooltip with this behavior appears
- **AND** existing view behavior remains unchanged

#### Scenario: Horizontal bar tooltip unchanged
- **GIVEN** the horizontal bar tooltip is already working correctly
- **WHEN** user hovers or clicks the horizontal bar
- **THEN** behavior remains exactly as before
- **AND** no modifications are made to horizontal bar tooltip
