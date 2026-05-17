using System;
using System.Collections.Generic;
using System.Text;
using Utility;

namespace WorkFocusManager.Models
{
    public class TimerProcessingLogModel : ViewModelBase
    {
        private int id = 1;
        public int Id
        {
            get => id;
            set => Set(ref id, value);
        }
        private string title;
        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        private DateTime loggingDate;
        public DateTime LoggingDate
        {
            get => loggingDate;
            set => Set(ref loggingDate, value);
        }

        private DateTime startTime;
        public DateTime StartTime
        {
            get => startTime;
            set => Set(ref startTime, value);
        }

        private DateTime endTime;
        public DateTime EndTime
        {
            get => endTime;
            set
            {
                Set(ref endTime, value);

                TimeSpan diff = (value - StartTime).Duration();
                DurationTime = $"{(int)diff.TotalHours}시간 {diff.Minutes}분 {diff.Seconds}초";
            }
        }

        public string durationTime;
        public string DurationTime
        {
            get => durationTime;
            set => Set(ref durationTime, value);
        }

        public int blockProcessCount;
        public int BlockProcessCount
        {
            get => blockProcessCount;
            set => Set(ref blockProcessCount, value);
        }
    }
}
