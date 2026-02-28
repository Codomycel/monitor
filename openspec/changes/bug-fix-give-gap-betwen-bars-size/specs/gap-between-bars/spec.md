## Requirement: Equal gaps with minimum bar width (general for 1/2/3 bars)

The chart SHALL keep bars readable (not too thin) and distribute remaining width as equal spacing within each category slot.

### Definitions
- `slotCount` = number of X-axis categories (slots)
- `barsPerSlot` = number of visible bars per slot (1..3)
- `slotWidth = plotWidth / slotCount`

### Scenario: Equal spacing inside a slot
- GIVEN `slotWidth = W` and `barsPerSlot = N`
- WHEN sizing is updated
- THEN spacing SHALL follow:

  `gap = (W - N*barWidth) / (N + 1)`

- AND left outer space, between-bar spaces, and right outer space SHALL all be equal to `gap`.

### Scenario: Bar width minimum
- WHEN sizing is updated
- THEN `barWidth` SHALL be >= `MinBarWidth`.

### Scenario: Minimum gap preference
- GIVEN `MinGap`
- WHEN computed `gap < MinGap`
- THEN the implementation SHALL attempt to reduce `barWidth` (but not below `MinBarWidth`) to achieve `gap >= MinGap` where possible.

### Scenario: Narrow width safety
- WHEN available width is too small to satisfy both MinBarWidth and MinGap
- THEN bars SHALL remain visible and gaps SHALL clamp to safe values (no overlap/negative sizing).

### Scenario: Update without data recreation
- WHEN gap/width changes or chart is resized
- THEN the chart SHALL update sizing for existing visible `ColumnSeries` without recreating the underlying data.