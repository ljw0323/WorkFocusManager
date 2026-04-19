using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WorkFocusManager.Models;

namespace WorkFocusManager.Utility
{
    public static class ProcessStatusManager
    {
        public static List<ProcessModel> GetProcessList()
        {
            List<ProcessModel> processModels = new List<ProcessModel>();
            try
            {
                var processList = Process.GetProcesses().OrderBy(x => x.ProcessName).ToList();

                foreach(var process in processList)
                {
                    var processModel = new ProcessModel()
                    {
                        Id = process.Id,
                        ProcessName = process.ProcessName,
                        ProcessIcon = GetProcessIcon(process),
                        UsingMemorySize = $"{process.WorkingSet64 / 1024 / 1024} MB"
                    };

                    processModels.Add(processModel);
                }


                processModels = processModels;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return processModels;
        }

        public static BitmapSource GetProcessIcon(Process process)
        {
            try
            {
                string path = process.MainModule.FileName;

                if (!File.Exists(path))
                    return null;

                Icon icon = Icon.ExtractAssociatedIcon(path);

                if (icon == null)
                    return null;

                return Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return null;
            }
        }
    }
}
