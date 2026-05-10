using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using WorkFocusManager.Models;

namespace WorkFocusManager.Configs
{
    public class SystemConfig : ViewModelBase
    {
        protected static SystemConfig _instance;
        public static SystemConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SystemConfig();

                return _instance;
            }
        }

        public string Name { get; set; }

        private List<ProcessGroupModel> processGroupModelBlackList;
        public List<ProcessGroupModel> ProcessGroupModelBlackList
        {
            get => processGroupModelBlackList;
            set => Set(ref processGroupModelBlackList, value);
        }

        public List<ProcessModel> processModelBlackList;
        public List<ProcessModel> ProcessModelBlackList
        {
            get => processModelBlackList;
            set => Set(ref processModelBlackList, value);
        }
    }
}
