using System.Windows.Media.Imaging;
using Utility;

namespace WorkFocusManager.Models
{
    public enum ProcessCategory
    {
        App,
        Background
    }

    public class ProcessModel : ViewModelBase
    {
        private bool isBlocked;
        private bool isWhiteListed;
        private string note = string.Empty;

        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public BitmapSource? ProcessIcon { get; set; }
        public long UsingMemoryBytes { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public bool IsBlocked
        {
            get => isBlocked;
            set => Set(ref isBlocked, value);
        }

        public bool IsWhiteListed
        {
            get => isWhiteListed;
            set => Set(ref isWhiteListed, value);
        }

        public string Note
        {
            get => note;
            set => Set(ref note, value);
        }

        public string UsingMemorySize =>
            $"{UsingMemoryBytes / 1024 / 1024} MB";
    }

    public class ProcessGroupModel : ViewModelBase
    {
        private bool isBlocked;
        private string displayName = string.Empty;
        private string note = string.Empty;

        public string ProcessName { get; set; } = string.Empty;
        public int Count { get; set; }
        public BitmapSource? ProcessIcon { get; set; }
        public long TotalMemoryBytes { get; set; }
        public List<ProcessModel> Items { get; set; } = new();

        public bool IsBlocked
        {
            get => isBlocked;
            set => Set(ref isBlocked, value);
        }

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace(displayName) ? ProcessName : displayName;
            set
            {
                if (Set(ref displayName, value))
                    OnPropertyChanged();
            }
        }

        public string Note
        {
            get => note;
            set => Set(ref note, value);
        }

        public string TotalMemorySize =>
            $"{TotalMemoryBytes / 1024 / 1024} MB";
    }

    public class ProcessCategoryGroupModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<ProcessGroupModel> Items { get; set; } = new();
    }
}
