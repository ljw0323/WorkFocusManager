using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Collections.Concurrent;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WorkFocusManager.Models;

namespace WorkFocusManager.Utility
{
    public static class ProcessStatusManager
    {
        private const string AppCategoryName = "\uC571";
        private const string BackgroundCategoryName = "\uBC31\uADF8\uB77C\uC6B4\uB4DC \uD504\uB85C\uC138\uC2A4";
        private static readonly ConcurrentDictionary<string, BitmapSource?> IconCache = new();

        public static List<ProcessCategoryGroupModel> GetProcessCategoryGroupList()
        {
            return GetProcessList()
                .GroupBy(x => x.CategoryName)
                .Select(categoryGroup => new ProcessCategoryGroupModel
                {
                    CategoryName = categoryGroup.Key,
                    Count = categoryGroup.Count(),
                    Items = categoryGroup
                        .GroupBy(x => x.ProcessName)
                        .Select(processGroup => new ProcessGroupModel
                        {
                            ProcessName = processGroup.Key,
                            Count = processGroup.Count(),
                            ProcessIcon = processGroup.FirstOrDefault(x => x.ProcessIcon != null)?.ProcessIcon,
                            TotalMemoryBytes = processGroup.Sum(x => x.UsingMemoryBytes),
                            Items = processGroup
                                .OrderBy(x => x.Id)
                                .ToList()
                        })
                        .OrderByDescending(x => x.TotalMemoryBytes)
                        .ToList()
                })
                .OrderBy(x => x.CategoryName == AppCategoryName ? 0 : 1)
                .ToList();
        }

        public static List<ProcessModel> GetProcessList()
        {
            var processModels = new List<ProcessModel>();

            try
            {
                foreach (var process in Process.GetProcesses().OrderBy(x => x.ProcessName))
                {
                    try
                    {
                        processModels.Add(CreateProcessModel(process));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return processModels;
        }

        public static BlacklistKillResult KillBlacklistedProcesses(
            ISet<string> blockedGroupNames,
            ISet<int> blockedProcessIds,
            ISet<string> blockedProcessNames,
            ISet<string> whiteListProcessNames,
            ISet<int> ignoredProcessIds,
            ISet<string> allowedTargetKeys)
        {
            var currentProcessId = Environment.ProcessId;
            var result = new BlacklistKillResult();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id == currentProcessId || ignoredProcessIds.Contains(process.Id) || whiteListProcessNames.Contains(process.ProcessName))
                        continue;

                    var targetKey = GetBlacklistTargetKey(
                        process,
                        blockedGroupNames,
                        blockedProcessIds,
                        blockedProcessNames);

                    if (targetKey == null || !allowedTargetKeys.Contains(targetKey))
                        continue;

                    process.Kill(entireProcessTree: true);
                    result.KilledProcessIds.Add(process.Id);
                    result.BlockedTargets[targetKey] = new BlockedProcessLogModel
                    {
                        BlockedAt = DateTime.Now,
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Reason = targetKey.StartsWith("group:")
                            ? "\uADF8\uB8F9 \uCC28\uB2E8"
                            : "\uD504\uB85C\uC138\uC2A4 \uCC28\uB2E8"
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }

            return result;
        }

        public static List<BlacklistProcessTarget> GetBlacklistedProcessTargets(
            ISet<string> blockedGroupNames,
            ISet<int> blockedProcessIds,
            ISet<string> blockedProcessNames,
            ISet<string> whiteListProcessNames,
            ISet<int> ignoredProcessIds)
        {
            var currentProcessId = Environment.ProcessId;
            var result = new List<BlacklistProcessTarget>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id == currentProcessId || ignoredProcessIds.Contains(process.Id) || whiteListProcessNames.Contains(process.ProcessName))
                        continue;

                    var targetKey = GetBlacklistTargetKey(
                        process,
                        blockedGroupNames,
                        blockedProcessIds,
                        blockedProcessNames);

                    if (targetKey == null)
                        continue;

                    result.Add(new BlacklistProcessTarget
                    {
                        TargetKey = targetKey,
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    process.Dispose();
                }
            }

            return result;
        }

        private static string? GetBlacklistTargetKey(
            Process process,
            ISet<string> blockedGroupNames,
            ISet<int> blockedProcessIds,
            ISet<string> blockedProcessNames)
        {
            if (blockedProcessIds.Contains(process.Id))
                return $"pid:{process.Id}";

            if (blockedGroupNames.Contains(process.ProcessName))
                return $"group:{process.ProcessName}";

            if (blockedProcessNames.Contains(process.ProcessName))
                return $"process:{process.ProcessName}";

            return null;
        }

        private static ProcessModel CreateProcessModel(Process process)
        {
            var hasMainWindow = process.MainWindowHandle != IntPtr.Zero;

            return new ProcessModel
            {
                Id = process.Id,
                ProcessName = process.ProcessName,
                ProcessIcon = hasMainWindow ? GetProcessIcon(process) : null,
                UsingMemoryBytes = GetWorkingSet(process),
                CategoryName = hasMainWindow? AppCategoryName: BackgroundCategoryName
            };
        }

        private static long GetWorkingSet(Process process)
        {
            try
            {
                return process.WorkingSet64;
            }
            catch
            {
                return 0;
            }
        }

        private static BitmapSource? GetProcessIcon(Process process)
        {
            try
            {
                var path = process.MainModule?.FileName;

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return null;

                if (IconCache.TryGetValue(path, out var cachedIcon))
                    return cachedIcon;

                using var icon = Icon.ExtractAssociatedIcon(path);

                if (icon == null)
                {
                    IconCache[path] = null;
                    return null;
                }

                var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(24, 24));

                bitmapSource.Freeze();
                IconCache[path] = bitmapSource;

                return bitmapSource;
            }
            catch
            {
                return null;
            }
        }
    }

    public class BlacklistKillResult
    {
        public HashSet<int> KilledProcessIds { get; } = new();
        public Dictionary<string, BlockedProcessLogModel> BlockedTargets { get; } = new();
    }

    public class BlacklistProcessTarget
    {
        public string TargetKey { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
    }
}
