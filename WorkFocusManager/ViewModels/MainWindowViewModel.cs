using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utility;
using WorkFocusManager.Configs;
using WorkFocusManager.Models;
using WorkFocusManager.Utility;

namespace WorkFocusManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const int ProcessRefreshIntervalSeconds = 5;
        private const int BlockGraceSeconds = 10;

        private readonly DispatcherTimer timer;
        private readonly HashSet<int> countedKilledProcessIds = new();
        private readonly HashSet<string> countedBlockedTargetKeys = new();
        private readonly Dictionary<string, PendingBlockWarningModel> pendingBlockWarningsByKey = new();

        private TimeSpan totalTime;
        private TimeSpan remainingTime;
        private bool isRunning;
        private bool isPaused;
        private bool isBreakMode;
        private bool isShowDetailView;
        private string timerText = string.Empty;
        private string timerModeText = "\uC9D1\uC911";
        private DateTime? selectedDate = DateTime.Now;
        private int selectedHour;
        private int selectedMinute;
        private int selectedSecond;
        private long timerTick;
        private bool isEnforcingBlacklist;
        private bool isRefreshingProcessGroups;
        private bool hasLoadedProcessGroups;
        private TimerProcessingLogModel? currentProcessingModel;

        private ObservableCollection<TimerProcessingLogModel> timerProcessingLogModelColleciton = new();
        private ObservableCollection<PendingBlockWarningModel> pendingBlockWarnings = new();
        private UsageStatisticsModel todayUsageStatistics = new();
        private UsageStatisticsModel monthlyUsageStatistics = new();
        private MonthlyReportModel monthlyReport = new();
        private List<ProcessCategoryGroupModel> processGroupModels = new();
        private Dictionary<DateTime, TimeSpan> calendarHighlightedDurations = new();

        private ICommand? controlTimercommand;
        private ICommand? resetTimercommand;
        private ICommand? showDetailViewcommand;
        private ICommand? addBlacklistCommand;
        private ICommand? removeBlacklistCommand;
        private ICommand? addWhiteListCommand;
        private ICommand? removeWhiteListCommand;
        private ICommand? pauseTimerCommand;
        private ICommand? saveConfigCommand;
        private ICommand? setTimerPresetCommand;

        public MainWindowViewModel()
        {
            totalTime = TimeSpan.FromMinutes(1);
            RemainingTime = totalTime;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

            Hours = new ObservableCollection<int>(Enumerable.Range(0, 24));
            Minutes = new ObservableCollection<int>(Enumerable.Range(0, 60));
            Seconds = new ObservableCollection<int>(Enumerable.Range(0, 60));

            SelectedHour = 0;
            SelectedMinute = 1;
            SelectedSecond = 0;

            UpdateSelectedTime();
            EnsureConfigCollections();
            LoadUsageRecords();
            RefreshProcessGroupsAsync();
        }

        public SystemConfig SystemConfig => SystemConfig.Instance;

        public TimeSpan RemainingTime
        {
            get => remainingTime;
            set
            {
                Set(ref remainingTime, value);
                TimerText = value.ToString(@"hh\:mm\:ss");
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set => Set(ref isRunning, value);
        }

        public bool IsPaused
        {
            get => isPaused;
            set => Set(ref isPaused, value);
        }

        public bool IsBreakMode
        {
            get => isBreakMode;
            set => Set(ref isBreakMode, value);
        }

        public bool IsShowDetailView
        {
            get => isShowDetailView;
            set => Set(ref isShowDetailView, value);
        }

        public string TimerText
        {
            get => timerText;
            set => Set(ref timerText, value);
        }

        public string TimerModeText
        {
            get => timerModeText;
            set => Set(ref timerModeText, value);
        }

        public DateTime? SelectedDate
        {
            get => selectedDate;
            set
            {
                if (Set(ref selectedDate, value))
                    LoadUsageRecords();
            }
        }

        public ObservableCollection<int> Hours { get; }
        public ObservableCollection<int> Minutes { get; }
        public ObservableCollection<int> Seconds { get; }

        public int SelectedHour
        {
            get => selectedHour;
            set
            {
                Set(ref selectedHour, value);
                UpdateSelectedTime();
            }
        }

        public int SelectedMinute
        {
            get => selectedMinute;
            set
            {
                Set(ref selectedMinute, value);
                UpdateSelectedTime();
            }
        }

        public int SelectedSecond
        {
            get => selectedSecond;
            set
            {
                Set(ref selectedSecond, value);
                UpdateSelectedTime();
            }
        }

        public ObservableCollection<TimerProcessingLogModel> TimerProcessingLogModelColleciton
        {
            get => timerProcessingLogModelColleciton;
            set => Set(ref timerProcessingLogModelColleciton, value);
        }

        public ObservableCollection<PendingBlockWarningModel> PendingBlockWarnings
        {
            get => pendingBlockWarnings;
            set => Set(ref pendingBlockWarnings, value);
        }

        public List<ProcessCategoryGroupModel> ProcessGroupModels
        {
            get => processGroupModels;
            set => Set(ref processGroupModels, value);
        }

        public UsageStatisticsModel TodayUsageStatistics
        {
            get => todayUsageStatistics;
            set => Set(ref todayUsageStatistics, value);
        }

        public UsageStatisticsModel MonthlyUsageStatistics
        {
            get => monthlyUsageStatistics;
            set => Set(ref monthlyUsageStatistics, value);
        }

        public MonthlyReportModel MonthlyReport
        {
            get => monthlyReport;
            set => Set(ref monthlyReport, value);
        }

        public string TodayGoalText
        {
            get
            {
                var goal = TimeSpan.FromMinutes(SystemConfig.TodayGoalMinutes);
                return $"{TodayUsageStatistics.TotalDurationText} / {(int)goal.TotalHours}\uC2DC\uAC04 {goal.Minutes}\uBD84";
            }
        }

        public double TodayGoalProgress
        {
            get
            {
                if (SystemConfig.TodayGoalMinutes <= 0)
                    return 0;

                var progress = TodayUsageStatistics.TotalDuration.TotalMinutes / SystemConfig.TodayGoalMinutes;
                return Math.Max(0, Math.Min(100, progress * 100));
            }
        }

        public Dictionary<DateTime, TimeSpan> CalendarHighlightedDurations
        {
            get => calendarHighlightedDurations;
            set => Set(ref calendarHighlightedDurations, value);
        }

        public ICommand ControlTimercommand => controlTimercommand ??= new RelayCommand(ControlTimerAction);
        public ICommand ResetTimercommand => resetTimercommand ??= new RelayCommand(ResetTimerAction);
        public ICommand ShowDetailViewcommand => showDetailViewcommand ??= new RelayCommand(ShowDetailViewAction);
        public ICommand AddBlacklistCommand => addBlacklistCommand ??= new RelayCommand<object>(AddBlacklistAction);
        public ICommand RemoveBlacklistCommand => removeBlacklistCommand ??= new RelayCommand<object>(RemoveBlacklistAction);
        public ICommand AddWhiteListCommand => addWhiteListCommand ??= new RelayCommand<object>(AddWhiteListAction);
        public ICommand RemoveWhiteListCommand => removeWhiteListCommand ??= new RelayCommand<object>(RemoveWhiteListAction);
        public ICommand PauseTimerCommand => pauseTimerCommand ??= new RelayCommand(PauseTimerAction);
        public ICommand SaveConfigCommand => saveConfigCommand ??= new RelayCommand(SaveConfigAction);
        public ICommand SetTimerPresetCommand => setTimerPresetCommand ??= new RelayCommand<object>(SetTimerPresetAction);

        public void ToggleTimer()
        {
            ControlTimerAction();
        }

        public void ResetTimer()
        {
            ResetTimerAction();
        }

        private void UpdateSelectedTime()
        {
            totalTime = new TimeSpan(SelectedHour, SelectedMinute, SelectedSecond);

            if (!IsRunning && !IsPaused)
                RemainingTime = totalTime;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainingTime.TotalSeconds <= 0)
            {
                RemainingTime = TimeSpan.Zero;
                timer.Stop();
                IsRunning = false;
                IsPaused = false;

                if (IsBreakMode)
                {
                    FinishBreakTimer();
                    return;
                }

                CompleteCurrentProcessingLog();
                StartBreakTimerIfNeeded();
                return;
            }

            RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            if (!IsBreakMode)
                EnforceBlacklistAsync();

            if (IsShowDetailView && timerTick % ProcessRefreshIntervalSeconds == 0)
                RefreshProcessGroupsAsync();

            timerTick = timerTick > 1000000 ? 0 : timerTick + 1;
        }

        private void ControlTimerAction()
        {
            if (IsRunning)
            {
                PauseTimerAction();
                return;
            }

            if (remainingTime.TotalSeconds <= 0 || !IsPaused)
                RemainingTime = totalTime;

            timer.Start();
            IsRunning = true;
            IsPaused = false;

            if (!IsBreakMode)
            {
                TimerModeText = "\uC9D1\uC911";
                currentProcessingModel ??= new TimerProcessingLogModel
                {
                    Title = SystemConfig.StatusText,
                    LoggingDate = DateTime.Now,
                    StartTime = DateTime.Now
                };

                EnforceBlacklistAsync();
            }
        }

        private void PauseTimerAction()
        {
            if (!IsRunning)
                return;

            timer.Stop();
            IsRunning = false;
            IsPaused = true;
        }

        private void ResetTimerAction()
        {
            timer.Stop();
            IsRunning = false;
            IsPaused = false;
            IsBreakMode = false;
            TimerModeText = "\uC9D1\uC911";
            RemainingTime = totalTime;

            CompleteCurrentProcessingLog();
            OnPropertyChanged(nameof(TimerText));
        }

        private void ShowDetailViewAction()
        {
            IsShowDetailView = !IsShowDetailView;

            if (IsShowDetailView)
                RefreshProcessGroupsAsync();
        }

        private void AddBlacklistAction(object parameter)
        {
            var isChanged = false;

            switch (parameter)
            {
                case ProcessGroupModel processGroup when !IsGroupBlacklisted(processGroup.ProcessName):
                    SystemConfig.ProcessGroupModelBlackList.Add(processGroup);
                    isChanged = true;
                    break;

                case ProcessModel process when !IsProcessBlacklisted(process.ProcessName):
                    SystemConfig.ProcessModelBlackList.Add(process);
                    isChanged = true;
                    break;
            }

            ApplyBlacklistState();

            if (isChanged)
                SystemConfig.Save();
        }

        private void RemoveBlacklistAction(object parameter)
        {
            switch (parameter)
            {
                case ProcessGroupModel processGroup:
                    RemoveGroupBlacklist(processGroup.ProcessName);
                    break;

                case ProcessModel process:
                    RemoveProcessBlacklist(process);
                    break;
            }

            ApplyBlacklistState();
            SystemConfig.Save();
        }

        private void AddWhiteListAction(object parameter)
        {
            if (parameter is not ProcessModel process || IsProcessWhiteListed(process.ProcessName))
                return;

            process.IsWhiteListed = true;
            SystemConfig.ProcessModelWhiteList.Add(process);
            ApplyBlacklistState();
            SystemConfig.Save();
        }

        private void RemoveWhiteListAction(object parameter)
        {
            if (parameter is not ProcessModel process)
                return;

            var existing = SystemConfig.ProcessModelWhiteList
                .FirstOrDefault(x => x.ProcessName == process.ProcessName);

            if (existing != null)
                SystemConfig.ProcessModelWhiteList.Remove(existing);

            ApplyBlacklistState();
            SystemConfig.Save();
        }

        private void ApplyBlacklistState()
        {
            var blockedGroupNames = SystemConfig.ProcessGroupModelBlackList
                .Select(x => x.ProcessName)
                .ToHashSet();

            var blockedProcessIds = SystemConfig.ProcessModelBlackList
                .Select(x => x.Id)
                .Where(x => x > 0)
                .ToHashSet();

            var blockedProcessNames = SystemConfig.ProcessModelBlackList
                .Select(x => x.ProcessName)
                .ToHashSet();

            var whiteListProcessNames = SystemConfig.ProcessModelWhiteList
                .Select(x => x.ProcessName)
                .ToHashSet();

            foreach (var category in ProcessGroupModels)
            {
                foreach (var processGroup in category.Items)
                {
                    var isGroupBlocked = blockedGroupNames.Contains(processGroup.ProcessName);

                    processGroup.IsBlocked = isGroupBlocked;

                    foreach (var process in processGroup.Items)
                    {
                        process.IsWhiteListed = whiteListProcessNames.Contains(process.ProcessName);
                        process.IsBlocked = !process.IsWhiteListed
                            && (isGroupBlocked || blockedProcessIds.Contains(process.Id) || blockedProcessNames.Contains(process.ProcessName));
                    }
                }
            }
        }

        private void CompleteCurrentProcessingLog()
        {
            if (currentProcessingModel == null)
                return;

            currentProcessingModel.EndTime = DateTime.Now;
            TimerProcessingLogStore.Append(currentProcessingModel);

            if (SelectedDate?.Date == currentProcessingModel.LoggingDate.Date)
                LoadDailyLogs(currentProcessingModel.LoggingDate);

            RefreshTodayStatistics();
            RefreshMonthlyStatistics(currentProcessingModel.LoggingDate);
            currentProcessingModel = null;
            ClearBlockSessionState();
        }

        private void EnsureConfigCollections()
        {
            SystemConfig.ProcessGroupModelBlackList ??= new ObservableCollection<ProcessGroupModel>();
            SystemConfig.ProcessModelBlackList ??= new ObservableCollection<ProcessModel>();
            SystemConfig.ProcessModelWhiteList ??= new ObservableCollection<ProcessModel>();
        }

        private void LoadUsageRecords()
        {
            var targetDate = SelectedDate ?? DateTime.Now;

            LoadDailyLogs(targetDate);
            RefreshTodayStatistics();
            RefreshMonthlyStatistics(targetDate);
        }

        private void LoadDailyLogs(DateTime date)
        {
            var logs = TimerProcessingLogStore.LoadByDate(date)
                .OrderByDescending(x => x.StartTime)
                .ToList();

            TimerProcessingLogModelColleciton = new ObservableCollection<TimerProcessingLogModel>(logs);
        }

        private void RefreshMonthlyStatistics(DateTime date)
        {
            MonthlyUsageStatistics = TimerProcessingLogStore.LoadMonthlyStatistics(date);
            CalendarHighlightedDurations = TimerProcessingLogStore.LoadMonthlyDurations(date);
            MonthlyReport = CreateMonthlyReport(date);
        }

        private void RefreshTodayStatistics()
        {
            TodayUsageStatistics = TimerProcessingLogStore.LoadDailyStatistics(DateTime.Now);
            OnPropertyChanged(nameof(TodayGoalText));
            OnPropertyChanged(nameof(TodayGoalProgress));
        }

        private void RefreshProcessGroups(List<ProcessCategoryGroupModel> processGroups)
        {
            ProcessGroupModels = processGroups;
            hasLoadedProcessGroups = true;
            ApplyBlacklistState();
        }

        private void RefreshProcessGroupsAsync()
        {
            if (IsRunning && hasLoadedProcessGroups)
                return;

            if (isRefreshingProcessGroups)
                return;

            isRefreshingProcessGroups = true;

            Task.Run(() =>
            {
                var processGroups = ProcessStatusManager.GetProcessCategoryGroupList();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshProcessGroups(processGroups);
                    isRefreshingProcessGroups = false;
                });
            })
            .ContinueWith(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        isRefreshingProcessGroups = false;
                    });
                }
            });
        }

        private void StartBreakTimerIfNeeded()
        {
            if (SystemConfig.BreakMinutes <= 0)
                return;

            IsBreakMode = true;
            TimerModeText = "\uD734\uC2DD";
            RemainingTime = TimeSpan.FromMinutes(SystemConfig.BreakMinutes);
            timer.Start();
            IsRunning = true;
            IsPaused = false;
        }

        private void FinishBreakTimer()
        {
            IsBreakMode = false;
            TimerModeText = "\uC9D1\uC911";
            RemainingTime = totalTime;
        }

        private MonthlyReportModel CreateMonthlyReport(DateTime date)
        {
            var logs = TimerProcessingLogStore.LoadByMonth(date);
            var report = new MonthlyReportModel();

            if (logs.Count == 0)
                return report;

            var dailyDurations = logs
                .GroupBy(x => x.LoggingDate.Date)
                .Select(x => new
                {
                    Date = x.Key,
                    Duration = TimeSpan.FromTicks(x.Sum(log => log.Duration.Ticks))
                })
                .OrderByDescending(x => x.Duration)
                .ToList();

            var bestDay = dailyDurations.First();
            report.BestDayText = $"{bestDay.Date:MM/dd} {(int)bestDay.Duration.TotalHours}\uC2DC\uAC04 {bestDay.Duration.Minutes}\uBD84";

            var weekdays = logs
                .GroupBy(x => x.LoggingDate.DayOfWeek)
                .Select(x => new
                {
                    DayOfWeek = x.Key,
                    Duration = TimeSpan.FromTicks(x.Sum(log => log.Duration.Ticks))
                })
                .OrderByDescending(x => x.Duration)
                .First();

            report.BestWeekdayText = $"{ToKoreanWeekday(weekdays.DayOfWeek)} {(int)weekdays.Duration.TotalHours}\uC2DC\uAC04 {weekdays.Duration.Minutes}\uBD84";

            var topBlockedProcess = logs
                .SelectMany(x => x.BlockedProcesses)
                .GroupBy(x => x.ProcessName)
                .Select(x => new { ProcessName = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            report.TopBlockedProcessText = topBlockedProcess == null
                ? "-"
                : $"{topBlockedProcess.ProcessName} {topBlockedProcess.Count}\uD68C";

            report.FocusStreakDays = CalculateFocusStreak(dailyDurations.Select(x => x.Date).ToHashSet(), date);

            return report;
        }

        private static int CalculateFocusStreak(HashSet<DateTime> focusedDates, DateTime baseDate)
        {
            var cursor = baseDate.Date;
            var streak = 0;

            while (focusedDates.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            return streak;
        }

        private static string ToKoreanWeekday(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "\uC6D4",
                DayOfWeek.Tuesday => "\uD654",
                DayOfWeek.Wednesday => "\uC218",
                DayOfWeek.Thursday => "\uBAA9",
                DayOfWeek.Friday => "\uAE08",
                DayOfWeek.Saturday => "\uD1A0",
                _ => "\uC77C"
            };
        }

        private void EnforceBlacklistAsync()
        {
            if (!IsRunning || isEnforcingBlacklist)
                return;

            var blockedGroupNames = SystemConfig.ProcessGroupModelBlackList.Select(x => x.ProcessName).ToHashSet();
            var blockedProcessIds = SystemConfig.ProcessModelBlackList.Select(x => x.Id).Where(x => x > 0).ToHashSet();
            var blockedProcessNames = SystemConfig.ProcessModelBlackList.Select(x => x.ProcessName).ToHashSet();
            var whiteListProcessNames = SystemConfig.ProcessModelWhiteList.Select(x => x.ProcessName).ToHashSet();

            if (blockedGroupNames.Count == 0 && blockedProcessIds.Count == 0 && blockedProcessNames.Count == 0)
                return;

            if (SystemConfig.BlockMode == "Warning")
            {
                PendingBlockWarnings = new ObservableCollection<PendingBlockWarningModel>(
                    blockedGroupNames
                        .Concat(blockedProcessNames)
                        .Distinct()
                        .Select(x => new PendingBlockWarningModel
                        {
                            ProcessName = x,
                            KillAt = DateTime.Now.AddSeconds(5)
                        }));
                return;
            }

            isEnforcingBlacklist = true;
            var ignoredProcessIds = countedKilledProcessIds.ToHashSet();

            Task.Run(() =>
            {
                return ProcessStatusManager.GetBlacklistedProcessTargets(
                    blockedGroupNames,
                    blockedProcessIds,
                    blockedProcessNames,
                    whiteListProcessNames,
                    ignoredProcessIds);
            })
            .ContinueWith(task =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    isEnforcingBlacklist = false;

                    if (!task.IsCompletedSuccessfully || currentProcessingModel == null)
                        return;

                    UpdatePendingBlockWarnings(task.Result);
                });
            });
        }

        private void UpdatePendingBlockWarnings(List<BlacklistProcessTarget> targets)
        {
            var now = DateTime.Now;
            var currentKeys = targets.Select(x => x.TargetKey).ToHashSet();

            foreach (var staleKey in pendingBlockWarningsByKey.Keys.Except(currentKeys).ToList())
            {
                pendingBlockWarningsByKey.Remove(staleKey);
            }

            foreach (var target in targets)
            {
                if (pendingBlockWarningsByKey.ContainsKey(target.TargetKey))
                    continue;

                pendingBlockWarningsByKey[target.TargetKey] = new PendingBlockWarningModel
                {
                    TargetKey = target.TargetKey,
                    ProcessId = target.ProcessId,
                    ProcessName = target.ProcessName,
                    KillAt = now.AddSeconds(SystemConfig.BlockMode == "Strict" ? 0 : BlockGraceSeconds)
                };
            }

            var dueKeys = pendingBlockWarningsByKey
                .Where(x => x.Value.KillAt <= now)
                .Select(x => x.Key)
                .ToHashSet();

            PendingBlockWarnings = new ObservableCollection<PendingBlockWarningModel>(
                pendingBlockWarningsByKey.Values.OrderBy(x => x.KillAt));

            if (dueKeys.Count > 0)
                KillDueBlacklistedProcessesAsync(dueKeys);
        }

        private void KillDueBlacklistedProcessesAsync(HashSet<string> dueKeys)
        {
            var ignoredProcessIds = countedKilledProcessIds.ToHashSet();

            Task.Run(() =>
            {
                var blockedGroupNames = SystemConfig.ProcessGroupModelBlackList.Select(x => x.ProcessName).ToHashSet();
                var blockedProcessIds = SystemConfig.ProcessModelBlackList.Select(x => x.Id).Where(x => x > 0).ToHashSet();
                var blockedProcessNames = SystemConfig.ProcessModelBlackList.Select(x => x.ProcessName).ToHashSet();
                var whiteListProcessNames = SystemConfig.ProcessModelWhiteList.Select(x => x.ProcessName).ToHashSet();

                return ProcessStatusManager.KillBlacklistedProcesses(
                    blockedGroupNames,
                    blockedProcessIds,
                    blockedProcessNames,
                    whiteListProcessNames,
                    ignoredProcessIds,
                    dueKeys);
            })
            .ContinueWith(task =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!task.IsCompletedSuccessfully || currentProcessingModel == null)
                        return;

                    foreach (var processId in task.Result.KilledProcessIds)
                    {
                        countedKilledProcessIds.Add(processId);
                    }

                    foreach (var killedKey in task.Result.BlockedTargets.Keys)
                    {
                        pendingBlockWarningsByKey.Remove(killedKey);
                    }

                    var newBlockedLogs = task.Result.BlockedTargets
                        .Where(x => countedBlockedTargetKeys.Add(x.Key))
                        .Select(x => x.Value)
                        .ToList();

                    if (newBlockedLogs.Count > 0)
                    {
                        currentProcessingModel.BlockedProcesses.AddRange(newBlockedLogs);
                        currentProcessingModel.BlockProcessCount = currentProcessingModel.BlockedProcesses.Count;
                    }

                    PendingBlockWarnings = new ObservableCollection<PendingBlockWarningModel>(
                        pendingBlockWarningsByKey.Values.OrderBy(x => x.KillAt));
                });
            });
        }

        private void ClearBlockSessionState()
        {
            countedKilledProcessIds.Clear();
            countedBlockedTargetKeys.Clear();
            pendingBlockWarningsByKey.Clear();
            PendingBlockWarnings.Clear();
        }

        private void SetTimerPresetAction(object parameter)
        {
            if (IsRunning || IsPaused || !int.TryParse(parameter?.ToString(), out var totalMinutes))
                return;

            SelectedHour = totalMinutes / 60;
            SelectedMinute = totalMinutes % 60;
            SelectedSecond = 0;
            UpdateSelectedTime();
        }

        private bool IsGroupBlacklisted(string processName)
            => SystemConfig.ProcessGroupModelBlackList.Any(x => x.ProcessName == processName);

        private bool IsProcessBlacklisted(string processName)
            => SystemConfig.ProcessModelBlackList.Any(x => x.ProcessName == processName);

        private bool IsProcessWhiteListed(string processName)
            => SystemConfig.ProcessModelWhiteList.Any(x => x.ProcessName == processName);

        private void RemoveGroupBlacklist(string processName)
        {
            var existingGroup = SystemConfig.ProcessGroupModelBlackList
                .FirstOrDefault(x => x.ProcessName == processName);

            if (existingGroup != null)
                SystemConfig.ProcessGroupModelBlackList.Remove(existingGroup);
        }

        private void RemoveProcessBlacklist(ProcessModel process)
        {
            var existingProcess = SystemConfig.ProcessModelBlackList
                .FirstOrDefault(x => x.ProcessName == process.ProcessName);

            if (existingProcess != null)
            {
                SystemConfig.ProcessModelBlackList.Remove(existingProcess);
                return;
            }

            RemoveGroupBlacklist(process.ProcessName);
        }

        private void SaveConfigAction()
        {
            SystemConfig.Save();
            RefreshTodayStatistics();
        }
    }
}

