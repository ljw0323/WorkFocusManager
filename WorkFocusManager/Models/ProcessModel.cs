using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace WorkFocusManager.Models
{
    public enum ProcessCategory
    {
        App,
        Background
    }

    public class ProcessModel
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public BitmapSource ProcessIcon { get; set; }
        public long UsingMemoryBytes { get; set; }
        public string CategoryName { get; set; }

        public bool IsBlocked { get; set; }

        public string UsingMemorySize =>
            $"{UsingMemoryBytes / 1024 / 1024} MB";
    }

    public class ProcessGroupModel
    {
        public string ProcessName { get; set; }
        public int Count { get; set; }
        public BitmapSource ProcessIcon { get; set; }
        public bool IsBlocked { get; set; }
        public long TotalMemoryBytes { get; set; }

        public string TotalMemorySize =>
            $"{TotalMemoryBytes / 1024 / 1024} MB";

        public List<ProcessModel> Items { get; set; }
    }

    public class ProcessCategoryGroupModel
    {
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public List<ProcessGroupModel> Items { get; set; }
    }
}
