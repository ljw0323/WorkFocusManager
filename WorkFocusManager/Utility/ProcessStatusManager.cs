using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WorkFocusManager.Models;

namespace WorkFocusManager.Utility
{
    public static class ProcessStatusManager
    {
        public static List<ProcessCategoryGroupModel> GetProcessCategoryGroupList()
        {
            var processModels = GetProcessList();

            return processModels
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
                        .OrderBy(x => x.ProcessName)
                        .ToList()
                })
                .OrderBy(x => x.CategoryName == "앱" ? 0 : 1)
                .ToList();
        }

        public static List<ProcessModel> GetProcessList()
        {
            var processModels = new List<ProcessModel>();

            try
            {
                var processList = Process.GetProcesses()
                    .OrderBy(x => x.ProcessName)
                    .ToList();

                foreach (var process in processList)
                {
                    try
                    {
                        var isApp = process.MainWindowHandle != IntPtr.Zero;

                        var processModel = new ProcessModel
                        {
                            Id = process.Id,
                            ProcessName = process.ProcessName,
                            ProcessIcon = GetProcessIcon(process),
                            UsingMemoryBytes = GetWorkingSet(process),
                            CategoryName = process.MainWindowHandle != IntPtr.Zero
                            ? "앱"
                            : "백그라운드 프로세스",
                        };

                        processModels.Add(processModel);
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

        public static BitmapSource GetProcessIcon(Process process)
        {
            try
            {
                var path = process.MainModule?.FileName;

                if (string.IsNullOrWhiteSpace(path))
                    return null;

                if (!File.Exists(path))
                    return null;

                using (Icon icon = Icon.ExtractAssociatedIcon(path))
                {
                    if (icon == null)
                        return null;

                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(24, 24));

                    bitmapSource.Freeze();

                    return bitmapSource;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}