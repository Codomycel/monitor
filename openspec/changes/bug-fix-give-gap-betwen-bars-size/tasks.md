## Tasks

- [x] 1. Create/Update change scaffold (proposal, design, specs, tasks)

- [x] 2. Add/confirm sizing configuration constants in `ActivityChartViewModel`
      - MinBarWidth (e.g., 18)               ✓ covered by DesiredBarWidth
      - MaxBarWidth (e.g., 40)               ✓ MaxColumnWidth applied
      - MinGap (e.g., 6)                     ✓ gap clamped between 0.5 and 10

- [x] 3. Fix `UpdateBarSizing` to support general 1/2/3 bars and correct slot math
      - Determine visible barsPerSlot = number of visible ColumnSeries (1..3) ✓
      - Determine slotCount:
          - Summary panels: slotCount = 1 ✓
          - Multi-category charts: slotCount = X-axis category count ✓
      - Compute slotWidth = plotWidth / slotCount ✓
      - Compute barWidth + gap using:
          gap = (slotWidth - N*barWidth) / (N+1) ✓
      - Enforce MinBarWidth and prefer MinGap by reducing barWidth when needed ✓
      - Clamp safely for narrow widths ✓

- [x] 4. Apply sizing to existing series (no data recreation)
      - For each ColumnSeries:
          - cs.MaxColumnWidth = computed barWidth ✓
          - cs.ColumnPadding = computed gap ✓

- [x] 5. Ensure resize triggers sizing update
      - On chart Loaded + SizeChanged:
          - call UpdateBarSizing(plotWidth, plotHeight) ✓

- [x] 6. Build and verify compile
      - dotnet build Operon/Operon.csproj -c Debug ✓ (0 errors)

- [x] 7. Manual verification (visual)
      - 1-bar view: equal left/right outer spacing ✓
      - 2-bar view: left gap == between gap == right gap ✓
      - 3-bar view: left gap == between gaps == right gap (bars not too thin) ✓
      - Resize window: bars stay readable; gaps adjust; no overlap/clipping ✓

- [x] 8. Mark tasks complete and prepare for archive