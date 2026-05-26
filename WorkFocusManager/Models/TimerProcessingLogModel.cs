using Utility;

namespace WorkFocusManager.Models
{
    public class TimerProcessingLogModel : ViewModelBase
    {
        private int id = 1;
        private string title = string.Empty;
        private DateTime loggingDate;
        private DateTime startTime;
        private DateTime endTime;
        private string durationTime = string.Empty;
        private int blockProcessCount;
        private List<BlockedProcessLogModel> blockedProcesses = new();

        public int Id
        {
            get => id;
            set => Set(ref id, value);
        }

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        public DateTime LoggingDate
        {
            get => loggingDate;
            set => Set(ref loggingDate, value);
        }

        public DateTime StartTime
        {
            get => startTime;
            set => Set(ref startTime, value);
        }

        public DateTime EndTime
        {
            get => endTime;
            set
            {
                Set(ref endTime, value);

                var diff = (value - StartTime).Duration();
                DurationTime = $"{(int)diff.TotalHours}\uC2DC\uAC04 {diff.Minutes}\uBD84 {diff.Seconds}\uCD08";
            }
        }

        public string DurationTime
        {
            get => durationTime;
            set => Set(ref durationTime, value);
        }

        public int BlockProcessCount
        {
            get => blockProcessCount;
            set => Set(ref blockProcessCount, value);
        }

        public List<BlockedProcessLogModel> BlockedProcesses
        {
            get => blockedProcesses;
            set => Set(ref blockedProcesses, value);
        }

        public TimeSpan Duration =>
            EndTime > StartTime ? EndTime - StartTime : TimeSpan.Zero;
    }

    public class BlockedProcessLogModel
    {
        public DateTime BlockedAt { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class UsageStatisticsModel : ViewModelBase
    {
        private int sessionCount;
        private TimeSpan totalDuration;
        private int blockProcessCount;
        private TimeSpan longestDuration;

        public int SessionCount
        {
            get => sessionCount;
            set => Set(ref sessionCount, value);
        }

        public TimeSpan TotalDuration
        {
            get => totalDuration;
            set
            {
                Set(ref totalDuration, value);
                OnPropertyChanged(nameof(TotalDurationText));
            }
        }

        public int BlockProcessCount
        {
            get => blockProcessCount;
            set => Set(ref blockProcessCount, value);
        }

        public TimeSpan LongestDuration
        {
            get => longestDuration;
            set
            {
                Set(ref longestDuration, value);
                OnPropertyChanged(nameof(LongestDurationText));
            }
        }

        public string TotalDurationText =>
            $"{(int)TotalDuration.TotalHours}\uC2DC\uAC04 {TotalDuration.Minutes}\uBD84";

        public string LongestDurationText =>
            $"{(int)LongestDuration.TotalHours}\uC2DC\uAC04 {LongestDuration.Minutes}\uBD84 {LongestDuration.Seconds}\uCD08";
    }

    public class MonthlyReportModel : ViewModelBase
    {
        private string bestDayText = "-";
        private string bestWeekdayText = "-";
        private string topBlockedProcessText = "-";
        private int focusStreakDays;

        public string BestDayText
        {
            get => bestDayText;
            set => Set(ref bestDayText, value);
        }

        public string BestWeekdayText
        {
            get => bestWeekdayText;
            set => Set(ref bestWeekdayText, value);
        }

        public string TopBlockedProcessText
        {
            get => topBlockedProcessText;
            set => Set(ref topBlockedProcessText, value);
        }

        public int FocusStreakDays
        {
            get => focusStreakDays;
            set
            {
                Set(ref focusStreakDays, value);
                OnPropertyChanged(nameof(FocusStreakText));
            }
        }

        public string FocusStreakText => $"{FocusStreakDays}\uC77C";
    }

    public class PendingBlockWarningModel : ViewModelBase
    {
        private string processName = string.Empty;
        private DateTime killAt;

        public string TargetKey { get; set; } = string.Empty;
        public int ProcessId { get; set; }

        public string ProcessName
        {
            get => processName;
            set => Set(ref processName, value);
        }

        public DateTime KillAt
        {
            get => killAt;
            set
            {
                Set(ref killAt, value);
                OnPropertyChanged(nameof(RemainingSecondsText));
            }
        }

        public string RemainingSecondsText =>
            $"{Math.Max(0, (int)Math.Ceiling((KillAt - DateTime.Now).TotalSeconds))}\uCD08 \uD6C4 \uC885\uB8CC";
    }
}
