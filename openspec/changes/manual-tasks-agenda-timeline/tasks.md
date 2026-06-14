## 1. Inspect Existing Manual Tasks Implementation

- [x] 1.1 Locate Manual Tasks tab UI in UiAMainWindow.xaml (TabItem at line 917)
- [x] 1.2 Identify existing ManualTasksViewModel and its structure (MainWindowViewModel contains ManualTasks properties and commands)
- [x] 1.3 Review existing Add/Edit/Delete task flows (PrimaryManualTaskCommand, BeginEditManualTaskCommand, DeleteManualTaskRowCommand)
- [x] 1.4 Identify task repository/service methods for CRUD operations (ManualTaskService with Load(date) and Save(date, tasks) methods)
- [x] 1.5 Document current date selection mechanism (ManualTasksDate property with DatePicker binding)

## 2. Create ViewModel Extensions

- [x] 2.1 Add TimelineViewMode enum (Date, Week, Month)
- [x] 2.2 Add TimelineViewMode property to ManualTasksViewModel with change notification
- [x] 2.3 Create TimelineDateGroup class (Date, TotalDuration, Tasks collection)
- [x] 2.4 Add ObservableCollection<TimelineDateGroup> TimelineItems property
- [x] 2.5 Add LoadTimelineForDate() method
- [x] 2.6 Add LoadTimelineForWeek() method (7 days from selected date)
- [x] 2.7 Add LoadTimelineForMonth() method (calendar month)
- [x] 2.8 Add RefreshTimelineCommand
- [x] 2.9 Add RefreshTimeline() method that reloads based on current ViewMode
- [x] 2.10 Add NavigatePreviousCommand with logic per view mode (day/week/month)
- [x] 2.11 Add NavigateNextCommand with logic per view mode (day/week/month)
- [x] 2.12 Add PeriodLabelFormatted property (date/week/month text for display)
- [x] 2.13 Add CurrentDate property with change notification for navigation state

## 3. Create XAML UI Components

- [x] 3.1 Add view mode selector (RadioButton group: Date/Week/Month) above existing content
- [x] 3.2 Add navigation bar with [<] [Period Label] [>] layout
- [x] 3.3 Bind navigation buttons to NavigatePreviousCommand and NavigateNextCommand
- [x] 3.4 Bind Period Label to PeriodLabelFormatted property
- [x] 3.5 Create Timeline ItemsControl with VirtualizingStackPanel
- [x] 3.6 Create DateGroup DataTemplate (date header + total duration)
- [x] 3.7 Create TaskRow DataTemplate (title, time/duration, Edit button, Delete button)
- [x] 3.8 Add empty state panel for Date view ("No manual tasks for this date")
- [x] 3.9 Bind view mode selector to TimelineViewMode property
- [x] 3.10 Bind ItemsControl to TimelineItems collection

## 4. Wire Up Task Actions

- [x] 4.1 Wire Edit button to existing Edit Task dialog (Timeline uses BeginEditManualTaskCommand with existing dialog)
- [x] 4.2 Wire Delete button to existing Delete confirmation (Timeline uses DeleteManualTaskRowCommand with existing confirmation)
- [x] 4.3 Ensure dialog result triggers RefreshTimeline() (PersistManualTasks calls RefreshTimeline)
- [x] 4.4 Wire Add Task success to trigger RefreshTimeline() (PrimaryManualTaskAction calls PersistManualTasks which calls RefreshTimeline)
- [x] 4.5 Handle date selection change to reload timeline (ManualTasksDate setter syncs TimelineCurrentDate)

## 5. Styling and Polish

- [x] 5.1 Apply existing theme colors to timeline components (using #E5E7EB, #F9FAFB, #374151, #059669)
- [x] 5.2 Set consistent spacing between date groups (Padding="0,12", Margin="0,0,0,8")
- [x] 5.3 Style task rows with hover effects matching existing UI (Background="#F9FAFB", CornerRadius="6")
- [x] 5.4 Ensure date format matches existing app conventions (StringFormat='{}{0:ddd, MMM d}')
- [x] 5.5 Ensure duration format matches `HHh MMm` pattern (TotalDurationText property in TimelineDateGroup)

## 6. Testing and Verification

- [ ] 6.1 Test Date view with tasks present
- [ ] 6.2 Test Date view with no tasks (empty state message)
- [ ] 6.3 Test Week view shows only dates with tasks
- [ ] 6.4 Test Month view shows only dates with tasks
- [ ] 6.5 Test Edit task from timeline refreshes correctly
- [ ] 6.6 Test Delete task from timeline refreshes correctly
- [ ] 6.7 Test Add task refreshes timeline correctly
- [ ] 6.8 Test switching views maintains correct data
- [ ] 6.9 Test Previous/Next navigation in Date mode
- [ ] 6.10 Test Previous/Next navigation in Week mode
- [ ] 6.11 Test Previous/Next navigation in Month mode
- [ ] 6.12 Test period label updates correctly with navigation
- [ ] 6.13 Verify existing Add/Edit/Delete dialogs unchanged
- [x] 6.14 Build and verify no compilation errors
