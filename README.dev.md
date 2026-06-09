# Operon – System Activity Tracker – Technical Documentation

## 1. Tech Stack

- Platform: Windows only
- Framework: WPF (.NET 8)
- Pattern: MVVM
- Target: net8.0-windows
- Storage: CSV + JSON

---

## 2. Activity States

- Active: user input present
- Idle: no input beyond idle threshold
- Locked: Windows session locked

State transitions are based on:
- Windows session events
- Idle time detection
- Active window polling

---

## 3. Data Persistence

### Settings
Path:
%LocalAppData%\SystemActivityTracker\settings.json

### Daily Logs
Pattern:
activity-log-YYYY-MM-DD.csv

Each row includes:
- StartTime
- EndTime
- ProcessName
- State flags

---

## 4. Aggregation Strategy

- Daily summary → single CSV
- Weekly summary → multiple daily CSVs
- Monthly summary → aggregated daily CSVs

Historical data is never rewritten.

---

## 5. Header Active Timer

- Counts only Active state
- Pauses during Idle or Locked
- Resumes automatically

---

## 6. Settings Application Flow

1. User edits settings
2. Clicks Save Settings
3. App:
   - Writes JSON
   - Applies thresholds
   - Restarts timers
   - Refreshes summaries

---

## 7. Live Refresh

Enabled only when:
- EnableLiveRefresh = true
- Tracking is running
- Selected date is today

---

## 8. Developer / Test Mode

### Force Write Now
- Flushes current segment to CSV
- Triggers summary refresh
- No historical modification

---

## 9. UI Structure

- Header: title, date, live indicator, exit
- Summary cards: Day, Week, Controls
- Tabs:
  - Application Usage
  - Weekly Overview
  - Monthly Usage
  - Settings

---

## 10. Tray Integration

- Window close hides to tray
- Explicit exit required
- Ensures clean shutdown

---

## 11. Design Decisions

- CSV for transparency
- Refresh-based aggregation
- Local-first architecture
