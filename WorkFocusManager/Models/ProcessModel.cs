using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace WorkFocusManager.Models
{
    public class ProcessModel
    {
        public BitmapSource ProcessIcon { get; set; }
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public string UsingMemorySize { get; set; }
    }
}
