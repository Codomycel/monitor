## ADDED Requirements

### Requirement: Calendar tile displays Total Active text
The calendar/month tile SHALL display Total Active hours text in the format `HHh MMm` (e.g., "06h 00m").

#### Scenario: Day has 6 hours active and 1 hour manual
- **WHEN** the calendar tile renders for a day with 6h Active and 1h Manual
- **THEN** the text "07h 00m" displays in the tile
- **AND** the text uses larger font (FontSize 14-16, FontWeight Bold)
- **AND** the text is centered horizontally and vertically in the tile

#### Scenario: Day has 45 minutes total active
- **WHEN** the calendar tile renders for a day with 45 minutes Total Active
- **THEN** the text "00h 45m" displays in the tile

#### Scenario: Day has more than 24 hours
- **WHEN** the calendar tile renders for a day with 25h 30m Total Active
- **THEN** the text "25h 30m" displays in the tile

### Requirement: Calendar tile shows Total Active progress bar
The calendar tile SHALL display a compact horizontal progress bar below the Total Active text, using 8 hours as the full width reference.

#### Scenario: 6 hours tracked (75% of 8h reference)
- **WHEN** Total Active is 6 hours
- **THEN** the bar fills to 75% of the reference width

#### Scenario: 8 hours tracked (100% of reference)
- **WHEN** Total Active is 8 hours
- **THEN** the bar fills completely (100%)

#### Scenario: 10 hours tracked (exceeds reference)
- **WHEN** Total Active is 10 hours
- **THEN** the bar extends beyond 100% or caps at full with different visual indicator
- **AND** the layout remains intact (no overflow)

#### Scenario: Zero hours tracked
- **WHEN** Total Active is 0 hours
- **THEN** the bar shows empty/background color only

### Requirement: Total Active text and bar are centered
The Total Active text and bar SHALL be centered both horizontally and vertically within the tile's main content area.

#### Scenario: Tile renders with data
- **WHEN** the tile has activity data
- **THEN** the text is horizontally centered in the tile
- **AND** the text is vertically centered with the bar below it
- **AND** the bar is horizontally centered below the text

### Requirement: Bar is compact
The horizontal bar SHALL be compact and not occupy the full tile width.

#### Scenario: Tile with standard size
- **WHEN** the tile renders
- **THEN** the bar has fixed height (6-8px)
- **AND** the bar has fixed or constrained width (not stretching full tile)
