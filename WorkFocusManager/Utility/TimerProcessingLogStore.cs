using System.IO;
using Newtonsoft.Json;
using WorkFocusManager.Models;

namespace WorkFocusManager.Utility
{
    public static class TimerProcessingLogStore
    {
        private static readonly string LogDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TimerLogs");

        public static List<TimerProcessingLogModel> LoadByDate(DateTime date)
        {
            var path = GetLogPath(date);

            if (!File.Exists(path))
                return new List<TimerProcessingLogModel>();

            try
            {
                var json = File.ReadAllText(path);
                var logs = JsonConvert.DeserializeObject<List<TimerProcessingLogModel>>(json);

                return logs ?? new List<TimerProcessingLogModel>();
            }
            catch
            {
                return new List<TimerProcessingLogModel>();
            }
        }

        public static void Append(TimerProcessingLogModel log)
        {
            Directory.CreateDirectory(LogDirectory);

            var logs = LoadByDate(log.LoggingDate);
            log.Id = logs.Count + 1;
            logs.Add(log);

            Save(log.LoggingDate, logs);
        }

        public static UsageStatisticsModel LoadDailyStatistics(DateTime date)
        {
            return CreateStatistics(LoadByDate(date));
        }

        public static UsageStatisticsModel LoadMonthlyStatistics(DateTime month)
        {
            return CreateStatistics(LoadByMonth(month));
        }

        public static List<TimerProcessingLogModel> LoadByMonth(DateTime month)
        {
            if (!Directory.Exists(LogDirectory))
                return new List<TimerProcessingLogModel>();

            var filePattern = $"{month:yyyy-MM}-*.json";

            return Directory
                .EnumerateFiles(LogDirectory, filePattern)
                .SelectMany(LoadByPath)
                .ToList();
        }

        public static Dictionary<DateTime, TimeSpan> LoadMonthlyDurations(DateTime month)
        {
            if (!Directory.Exists(LogDirectory))
                return new Dictionary<DateTime, TimeSpan>();

            var filePattern = $"{month:yyyy-MM}-*.json";

            return LoadByMonth(month)
                .GroupBy(x => x.LoggingDate.Date)
                .ToDictionary(
                    x => x.Key,
                    x => TimeSpan.FromTicks(x.Sum(log => log.Duration.Ticks)));
        }

        private static UsageStatisticsModel CreateStatistics(List<TimerProcessingLogModel> logs)
        {
            var stats = new UsageStatisticsModel();

            stats.SessionCount = logs.Count;
            stats.TotalDuration = TimeSpan.FromTicks(logs.Sum(x => x.Duration.Ticks));
            stats.BlockProcessCount = logs.Sum(x => x.BlockProcessCount);
            stats.LongestDuration = logs.Count == 0
                ? TimeSpan.Zero
                : logs.Max(x => x.Duration);

            return stats;
        }

        private static void Save(DateTime date, List<TimerProcessingLogModel> logs)
        {
            var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            File.WriteAllText(GetLogPath(date), json);
        }

        private static List<TimerProcessingLogModel> LoadByPath(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<TimerProcessingLogModel>>(json)
                    ?? new List<TimerProcessingLogModel>();
            }
            catch
            {
                return new List<TimerProcessingLogModel>();
            }
        }

        private static string GetLogPath(DateTime date)
            => Path.Combine(LogDirectory, $"{date:yyyy-MM-dd}.json");
    }
}
