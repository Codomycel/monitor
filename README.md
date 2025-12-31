# System Activity Tracker (Windows – WPF)

## 1. Overview

System Activity Tracker is a Windows-only desktop application built with WPF that monitors and summarizes your machine’s activity throughout the day.

It continuously classifies time into three states:

- **Active**: the system is in use (keyboard/mouse activity is present)
- **Idle**: the system is unlocked, but there has been no keyboard/mouse input for longer than the configured idle threshold
- **Locked**: the Windows session is locked

In addition to state-based time tracking, the app records **per-application usage** and provides daily, weekly, and monthly summaries.

This is designed for personal productivity tracking and lightweight monitoring (local-only, no cloud).

---

## 2. Key Features (must be exhaustive)

### Tracking

- **Active time (system in use)**
  - Time counted when the system is neither idle nor locked.
- **Idle time (based on idle threshold)**
  - Time counted when user inactivity exceeds `IdleThresholdMinutes`.
- **Locked time (system locked)**
  - Time counted while Windows is locked.
- **Day start time (first activity of the day)**
  - Computed from the earliest record start time found in the day’s log.
- **Day end time (last recorded activity)**
  - Computed from the latest record end time found in the day’s log.

### Summaries

- **Selected Day summary**
  - **Started time**
  - **Ended time**
  - **Active / Idle / Locked durations**
- **Weekly summary**
  - **Week number** (ISO week)
  - **From–To date range**
  - **Total Active / Idle / Locked**
- **Monthly usage summary**
  - **Per-application totals** across the selected month

### Application Usage

- **Per-app active duration (daily)**
  - The Application Usage tab shows per-process active time for the selected day.
- **Weekly per-app totals**
  - Weekly Overview shows per-day totals (Active/Idle/Locked) for the selected week.
  - Per-application weekly totals are not currently shown as a dedicated table; weekly rollups are derived from the daily logs.
- **Monthly per-app totals**
  - The Monthly Usage tab aggregates per-process totals for the selected month.

### Header

- **App name (left)**
- **Today’s date (center)**
- **Live running indicator (animated)**
  - Visual indicator reflects running/stopped status.
- **Active running timer (only counts ACTIVE time)**
  - The header timer is intended to increment only while the current state is Active (and freeze during Idle/Locked).

### Controls

- **Start Tracking**
- **Stop Tracking**
- **Refresh Summary**
  - Triggers the app’s standard refresh flow to recompute UI summaries.
- **Status indicator (Running / Stopped)**
- **Developer/Test mode**
- **Force Write Now (test mode only)**
  - Forces the current in-progress segment to be written to disk and refreshes summaries.

### Settings

- **Idle threshold (minutes)** (`IdleThresholdMinutes`)
- **Poll interval (seconds)** (`PollIntervalSeconds`)
- **Live refresh enable/disable** (`EnableLiveRefresh`)
- **Live refresh interval** (`LiveRefreshIntervalSeconds`)
- **Save Settings behavior**
  - Writes settings to JSON.
  - Applies settings to the tracking service immediately.
  - Triggers a summary refresh so Selected Day / This Week update immediately.
  - Re-applies live refresh timer start/stop and interval using the new values.

### System Tray

- **App minimizes to tray**
- **Closing window does NOT exit app**
  - Closing hides the window instead of shutting down.
- **Exit only via**
  - Tray menu
  - Dedicated close/exit button in the header

---

## 3. UI Layout

### Header

- Left: application title
- Center: selected date display
- Right: live timer + animated running indicator + exit button

### Summary cards (Day / Week / Controls)

- Card-based summary section designed to adapt responsively.

### Tabs

- **Application Usage**
- **Weekly Overview**
- **Monthly Usage**
- **Settings**

### Responsive layout

- Designed to adapt to smaller windows by wrapping/stacking card content rather than relying on fixed grid columns.
- DataGrids handle their own scrolling.

---

## 4. Sorting & Tables

All tables support:

- **Column sorting by clicking headers**
- **Visual sort indicators**
  - Ascending: **▲**
  - Descending: **▼**

Sorting is enabled and verified for:

- **Application Usage**
- **Weekly Overview**
- **Monthly Usage**

---

## 5. Behavior Details (important)

### Active timer behavior

- The header active timer is designed to **increment only while the current state is Active**.
- When the state becomes **Idle** or **Locked**, the timer should **freeze** and resume only when the state returns to Active.

### Settings changes

- Settings edits in textboxes/checkboxes update bound properties, but **behavior is applied only after clicking _Save Settings_**.
- After Save Settings:
  - settings are written to JSON
  - tracking service intervals/thresholds are applied
  - summaries are refreshed immediately
  - live refresh timer is restarted/stopped based on the new values

### When summaries refresh

Summaries are refreshed via the existing refresh flow:

- **On startup**
- **On date change**
- **On manual Refresh Summary**
- **After Save Settings**
- **On exit/start paths that trigger refresh for consistency**

### Live refresh behavior

- When `EnableLiveRefresh = true`, the app periodically refreshes summaries **only while**:
  - tracking is running
  - the selected date is **today**
- When disabled, the timer is stopped and the app will not auto-refresh.
- During refresh (manual or live), the app flushes the current in-progress tracking segment (today only) so the UI can reflect the latest totals.

---

## 6. Data Persistence

All data is stored locally under your Windows user profile.

### Settings

- **File**: `settings.json`
- **Location**: `%LocalAppData%\SystemActivityTracker\settings.json`

### Daily activity records

- **File pattern**: `activity-log-YYYY-MM-DD.csv`
- **Location**: `%LocalAppData%\SystemActivityTracker\activity-log-YYYY-MM-DD.csv`
- Each record includes start/end timestamps, application/process details, and state flags used to classify time into Active/Idle/Locked.

### Weekly aggregation

- Weekly totals and grids are computed by reading the daily CSV files for dates in the selected week.

### Monthly aggregation

- Monthly per-application totals are computed by reading all daily CSV files in the selected month and aggregating durations per process.

---

## 7. Developer / Test Mode

### Purpose

Developer/Test mode exists to help validate behavior without waiting for natural state transitions.

### Force Write Now

- Forces the current in-progress record to be flushed to the CSV.
- Immediately refreshes summaries so you can verify that aggregation and UI updates behave as expected.

### Safety notes

- This is intended for testing. It does not change schemas or rewrite historical records; it simply ensures the current segment is written immediately.

---

## 8. Limitations & Design Decisions

- **Windows-only**
  - Uses Windows APIs for session lock detection and active window/process sampling.
- **WPF-based**
  - Desktop UI built with XAML and WPF.
- **Local-only persistence**
  - No cloud sync, no multi-device support.
- **Refresh-based summaries**
  - Summaries are computed from persisted CSV data. This keeps aggregation deterministic and avoids mutating historical totals.
- **Historical data is not auto-modified**
  - The app does not rewrite past logs; it reads what is persisted and aggregates it.

---

## 9. Future Improvements (optional section)

- Charts and trends (daily/weekly/monthly)
- Export and import tools (CSV export bundles, filtered exports)
- Tray quick stats (today active/idle/locked at a glance)
- Dark mode
- Cloud sync / multi-device history (opt-in)

---

## 10. How to Run

### Requirements

- Windows 10/11
- .NET 8 (the project targets `net8.0-windows`)

### Start

- Open the solution/project in Visual Studio and run, **or** build and run the generated executable.
- If `AutoStartTrackingOnLaunch` is enabled in settings, tracking will start automatically.

### Exit properly

Because the window close button minimizes to tray, you must exit explicitly:

- Use the **header Exit button**, or
- Use the **tray icon menu** and choose **Exit**

The tray icon also supports restoring the main window (for example, via tray interaction such as double-click).

This ensures the app can flush any in-progress record cleanly.
