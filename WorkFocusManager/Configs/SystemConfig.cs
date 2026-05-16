using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using WorkFocusManager.Models;

namespace WorkFocusManager.Configs
{
    public class SystemConfig : ViewModelBase
    {
        private static readonly string ConfigPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "SystemConfig.json");

        protected static SystemConfig _instance;

        public static SystemConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();

                return _instance;
            }
        }

        private string statusText;
        public string StatusText
        {
            get => statusText;
            set => Set(ref statusText, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private List<ProcessGroupModel> processGroupModelBlackList;
        public List<ProcessGroupModel> ProcessGroupModelBlackList
        {
            get => processGroupModelBlackList;
            set => Set(ref processGroupModelBlackList, value);
        }

        private List<ProcessModel> processModelBlackList;
        public List<ProcessModel> ProcessModelBlackList
        {
            get => processModelBlackList;
            set => Set(ref processModelBlackList, value);
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(
                    this,
                    Formatting.Indented);

                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public static SystemConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return CreateDefault();

                var json = File.ReadAllText(ConfigPath);

                var config =
                    JsonConvert.DeserializeObject<SystemConfig>(json);

                return config ?? CreateDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);

                return CreateDefault();
            }
        }

        private static SystemConfig CreateDefault()
        {
            return new SystemConfig
            {
                StatusText = string.Empty,
                Name = string.Empty,
                ProcessGroupModelBlackList = new List<ProcessGroupModel>(),
                ProcessModelBlackList = new List<ProcessModel>()
            };
        }
    }
}