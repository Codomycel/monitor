# Proposal: Correct bar spacing (equal gaps) without making bars too thin

## Summary

Fix activity chart bar sizing so bars keep a minimum readable width and the remaining width is distributed as equal spacing (gap) between bars and at both ends.

## Problem

Current sizing makes bars too thin and does not create the expected spacing between bars. The chart shows large outer margins while bars appear stuck together.

## Objective

- Support 1, 2, or 3 bars per category slot.
- Keep bar width >= a minimum readable width.
- Distribute leftover width as equal spacing:
  - left outer space
  - gaps between bars
  - right outer space
- Apply sizing without recreating series data.
- Work for both:
  - summary panels (single slot, multiple bars)
  - multi-category charts (multiple slots)

## Non-Goals

- No change to data values or calculations.
- No redesign of colors, labels, or legends.