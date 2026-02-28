# Design: General sizing for 1/2/3 bars with equal spacing

## Terms

- `plotWidth`: the actual drawable width of the chart (available width for bars)
- `slotCount`: number of X-axis categories (slots)
- `barsPerSlot`: number of bars shown side-by-side in each slot (1..3)
- `slotWidth = plotWidth / slotCount`
- `barWidth (BW)`: width of a single bar
- `gap (G)`: spacing value that must be equal everywhere inside the slot

Equal spacing rule inside a slot:

- Left outer space = G
- Between each adjacent bar = G
- Right outer space = G

Total spaces in a slot = `barsPerSlot + 1`

## Required Math (per slot)

Given:
- `W = slotWidth`
- `N = barsPerSlot`
- `BW` chosen (respect min/max)

Compute:

- `G = (W - N*BW) / (N + 1)`

## Constraints

- `BW >= MinBarWidth`
- `BW <= MaxBarWidth`
- `G >= 0`

If `G < MinGap`, reduce BW but never below MinBarWidth:

- `BW2 = (W - (N + 1)*MinGap) / N`
- `BW = clamp(BW2, MinBarWidth, MaxBarWidth)`
- Recompute `G = (W - N*BW) / (N + 1)`
- If still negative (very narrow), clamp `G = 0`

## How to determine slotCount and barsPerSlot

### A) Summary panels (Selected Day / This Week)
- slotCount = 1 (single slot)
- barsPerSlot = number of visible series (1..3)

### B) Multi-category charts (e.g., many days in a week/month)
- slotCount = number of categories on X axis (labels/points)
- barsPerSlot = number of visible series (1..3)

## Applying to LiveCharts.Wpf (v0)

Update sizing on existing `ColumnSeries`:
- `cs.MaxColumnWidth = BW`
- `cs.ColumnPadding = G`

Important: sizing must be computed per slot. Using full plotWidth directly is only correct when slotCount = 1.

## Resize Behavior

On chart Loaded + SizeChanged:
- call `UpdateBarSizing(plotWidth, plotHeight)`
- sizing recalculates and updates existing series
- no series data recreation