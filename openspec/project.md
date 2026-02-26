# Project Context — System Activity Tracker (Operon)

## 1. Product Overview

System Activity Tracker is a Windows desktop application that tracks computer usage time and provides daily, weekly, and monthly summaries.

The system measures:
- Active time
- Idle time
- Locked time
- Manual task time

All data is stored locally. The application is private, offline-only, and does not send data externally.

---

## 2. Core Business Principles (Non-Negotiable)

1. No internet usage.
2. No telemetry.
3. No cloud sync (unless explicitly added via spec).
4. All tracking data must remain local.
5. Existing features must not break during new implementations.
6. Avoid large refactors unless explicitly requested.

---

## 3. Core Time Definitions

Active:
- User interacting (keyboard/mouse activity detected).

Idle:
- System unlocked but no activity beyond configured idle threshold.

Locked:
- Windows session locked.

Manual:
- User-entered task duration (HH/MM/SS).

Total Active:
Active + Manual

This definition must remain consistent across:
- Daily view
- Weekly view
- Monthly view
- All charts
- All summaries

---

## 4. Productivity Color Classification Rule

Total Active hours must be classified into color categories.

The same classification logic must be used everywhere:
- Daily chart
- Weekly chart
- Monthly calendar
- Monthly weekly totals

Color thresholds must be centralized in a single reusable service/helper.

Color logic must NOT be:
- Hardcoded in XAML
- Duplicated in multiple chart components
- Embedded separately in each view

If thresholds change in future, they must update system-wide automatically.

---

## 5. Monthly View Requirements

Monthly view contains:
- Calendar-based layout
- Total Active per day
- Weekly grouping

For each week inside a selected month:
- Calculate Weekly Total Active
- Weekly total must include only days within selected month
- Weekly total must follow the same color classification rule

---

## 6. Architecture Guardrails

Preferred pattern:
- MVVM (gradual adoption allowed)

Rules:
- Do not block UI thread.
- Background tracking must remain lightweight.
- Dispatcher must be used when updating UI from background threads.
- Avoid introducing performance-heavy loops.
- Avoid memory leaks (unsubscribe events, dispose timers properly).

---

## 7. Tracking Engine Rules

Tracking must:
- Remain lightweight
- Respect poll interval setting
- Respect idle threshold setting
- Pause correctly during lock state
- Resume correctly after unlock

Any modification to tracking logic must:
- Include reasoning
- Include edge-case validation (lock/unlock, sleep/resume)

---

## 8. Data Rules

- Do not modify storage schema unless explicitly requested.
- All time calculations must use centralized logic.
- Aggregation logic must not be duplicated across views.

---

## 9. Definition of Done (For Any Feature)

Every spec must include:
- What changed
- Files touched
- How to verify
- Edge cases considered
- Performance impact (if any)

---

## 10. AI Working Constraints

AI must:
- Keep changes localized
- Avoid renaming unrelated files
- Avoid structural refactors
- Follow existing patterns unless explicitly told otherwise
- Maintain privacy-first principle