using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Utility;
using WorkFocusManager.Models;

namespace WorkFocusManager.Configs
{
    public class SystemConfig : ViewModelBase
    {
        private static readonly string ConfigPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WorkFocusManager",
                "SystemConfig.json");

        private static readonly string LegacyConfigPath =
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

        private string characterKind = "Cat";
        public string CharacterKind
        {
            get => characterKind;
            set
            {
                if (Set(ref characterKind, string.IsNullOrWhiteSpace(value) ? "Cat" : value))
                    OnPropertyChanged(nameof(CharacterImagePath));
            }
        }

        public string CharacterImagePath => CharacterKind switch
        {
            "Dog" => "/Resources/DogImage.gif",
            "Bear" => "/Resources/BearImage.gif",
            _ => "/Resources/CatImage.gif"
        };

        private ObservableCollection<ProcessGroupModel> processGroupModelBlackList;
        public ObservableCollection<ProcessGroupModel> ProcessGroupModelBlackList
        {
            get => processGroupModelBlackList;
            set => Set(ref processGroupModelBlackList, value);
        }

        private ObservableCollection<ProcessModel> processModelBlackList;
        private ObservableCollection<ProcessModel> processModelWhiteList;
        public ObservableCollection<ProcessModel> ProcessModelBlackList
        {
            get => processModelBlackList;
            set => Set(ref processModelBlackList, value);
        }

        public ObservableCollection<ProcessModel> ProcessModelWhiteList
        {
            get => processModelWhiteList;
            set => Set(ref processModelWhiteList, value);
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

                var json = JsonConvert.SerializeObject(
                    ToPersistedConfig(),
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
            var paths = new[] { ConfigPath, LegacyConfigPath }
                .Where(File.Exists)
                .Distinct()
                .ToList();

            foreach (var path in paths)
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var persistedConfig = JsonConvert.DeserializeObject<PersistedSystemConfig>(json);

                    if (persistedConfig != null)
                        return FromPersistedConfig(persistedConfig);

                    var legacyConfig = JsonConvert.DeserializeObject<SystemConfig>(json);

                    if (legacyConfig != null)
                        return Normalize(legacyConfig);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return CreateDefault();
        }

        private static SystemConfig CreateDefault()
        {
            return new SystemConfig
            {
                StatusText = string.Empty,
                Name = string.Empty,
                CharacterKind = "Cat",
                ProcessGroupModelBlackList = new ObservableCollection<ProcessGroupModel>(),
                ProcessModelBlackList = new ObservableCollection<ProcessModel>(),
                ProcessModelWhiteList = new ObservableCollection<ProcessModel>()
            };
        }

        private static SystemConfig Normalize(SystemConfig config)
        {
            config.ProcessGroupModelBlackList ??= new ObservableCollection<ProcessGroupModel>();
            config.ProcessModelBlackList ??= new ObservableCollection<ProcessModel>();
            config.ProcessModelWhiteList ??= new ObservableCollection<ProcessModel>();

            return config;
        }

        private PersistedSystemConfig ToPersistedConfig()
        {
            return new PersistedSystemConfig
            {
                StatusText = StatusText,
                Name = Name,
                CharacterKind = CharacterKind,
                ProcessGroupModelBlackList = ProcessGroupModelBlackList?
                    .Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
                    .Select(x => new PersistedProcessGroup
                    {
                        ProcessName = x.ProcessName,
                        DisplayName = x.DisplayName,
                        Note = x.Note
                    })
                    .GroupBy(x => x.ProcessName)
                    .Select(x => x.First())
                    .ToList() ?? new List<PersistedProcessGroup>(),
                ProcessModelBlackList = ProcessModelBlackList?
                    .Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
                    .Select(x => new PersistedProcess
                    {
                        ProcessName = x.ProcessName,
                        Note = x.Note
                    })
                    .GroupBy(x => x.ProcessName)
                    .Select(x => x.First())
                    .ToList() ?? new List<PersistedProcess>(),
                ProcessModelWhiteList = ProcessModelWhiteList?
                    .Where(x => !string.IsNullOrWhiteSpace(x.ProcessName))
                    .Select(x => new PersistedProcess
                    {
                        ProcessName = x.ProcessName,
                        Note = x.Note
                    })
                    .GroupBy(x => x.ProcessName)
                    .Select(x => x.First())
                    .ToList() ?? new List<PersistedProcess>()
            };
        }

        private static SystemConfig FromPersistedConfig(PersistedSystemConfig persistedConfig)
        {
            return new SystemConfig
            {
                StatusText = persistedConfig.StatusText ?? string.Empty,
                Name = persistedConfig.Name ?? string.Empty,
                CharacterKind = string.IsNullOrWhiteSpace(persistedConfig.CharacterKind) ? "Cat" : persistedConfig.CharacterKind,
                ProcessGroupModelBlackList = new ObservableCollection<ProcessGroupModel>(
                    persistedConfig.ProcessGroupModelBlackList.Select(x => new ProcessGroupModel
                    {
                        ProcessName = x.ProcessName ?? string.Empty,
                        DisplayName = x.DisplayName ?? string.Empty,
                        Note = x.Note ?? string.Empty
                    })),
                ProcessModelBlackList = new ObservableCollection<ProcessModel>(
                    persistedConfig.ProcessModelBlackList.Select(x => new ProcessModel
                    {
                        ProcessName = x.ProcessName ?? string.Empty,
                        Note = x.Note ?? string.Empty
                    })),
                ProcessModelWhiteList = new ObservableCollection<ProcessModel>(
                    persistedConfig.ProcessModelWhiteList.Select(x => new ProcessModel
                    {
                        ProcessName = x.ProcessName ?? string.Empty,
                        Note = x.Note ?? string.Empty
                    }))
            };
        }

        private class PersistedSystemConfig
        {
            public string StatusText { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string CharacterKind { get; set; } = "Cat";
            public List<PersistedProcessGroup> ProcessGroupModelBlackList { get; set; } = new();
            public List<PersistedProcess> ProcessModelBlackList { get; set; } = new();
            public List<PersistedProcess> ProcessModelWhiteList { get; set; } = new();
        }

        private class PersistedProcessGroup
        {
            public string ProcessName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
        }

        private class PersistedProcess
        {
            public string ProcessName { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
        }
    }
}
