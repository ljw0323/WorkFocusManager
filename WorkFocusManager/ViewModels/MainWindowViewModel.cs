using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utility;
using WorkFocusManager.Configs;
using WorkFocusManager.Models;
using WorkFocusManager.Utility;
using WpfAnimatedGif;
using Newtonsoft.Json;

namespace WorkFocusManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DispatcherTimer timer;


        public SystemConfig SystemConfig => SystemConfig.Instance;

        private TimeSpan totalTime;

        private TimeSpan remainingTime;
        public TimeSpan RemainingTime
        {
            get => remainingTime;
            set
            {
                Set(ref remainingTime, value);
                TimerText = value.ToString(@"hh\:mm\:ss");
            }
        }

        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set => Set(ref isRunning, value);
        }

        private bool isShowDetailView;
        public bool IsShowDetailView
        {
            get => isShowDetailView;
            set => Set(ref isShowDetailView, value);
        }

        private string timerText;
        public string TimerText
        {
            get => timerText;
            set => Set(ref timerText, value);
        }

        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set => Set(ref selectedDate, value);
        }

        public ObservableCollection<int> Hours { get; set; }
        public ObservableCollection<int> Minutes { get; set; }
        public ObservableCollection<int> Seconds { get; set; }

        private int selectedHour;
        public int SelectedHour
        {
            get => selectedHour;
            set
            {
                Set(ref selectedHour, value);
                UpdateSelectedTime();
            }
        }

        private int selectedMinute;
        public int SelectedMinute
        {
            get => selectedMinute;
            set
            {
                Set(ref selectedMinute, value);
                UpdateSelectedTime();
            }
        }

        private int selectedSecond;
        public int SelectedSecond
        {
            get => selectedSecond;
            set
            {
                Set(ref selectedSecond, value);
                UpdateSelectedTime();
            }
        }

        private TimerProcessingLogModel CurrnetProcessingModel;

        private List<TimerProcessingLogModel> timerProcessingLogModels;
        public List<TimerProcessingLogModel> TimerProcessingLogModels
        {
            get => timerProcessingLogModels;
            set => Set(ref timerProcessingLogModels, value);
        }

        private List<ProcessCategoryGroupModel> processGroupModels;
        public List<ProcessCategoryGroupModel> ProcessGroupModels
        {
            get => processGroupModels;
            set => Set(ref processGroupModels, value);
        }

        public MainWindowViewModel()
        {

            totalTime = TimeSpan.FromMinutes(1);
            RemainingTime = totalTime;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            ProcessGroupModels = ProcessStatusManager.GetProcessCategoryGroupList();

            Hours = new ObservableCollection<int>(
                Enumerable.Range(0, 24));

            Minutes = new ObservableCollection<int>(
                Enumerable.Range(0, 60));

            Seconds = new ObservableCollection<int>(
                Enumerable.Range(0, 60));

            SelectedHour = 0;
            SelectedMinute = 1;
            SelectedSecond = 0;

            UpdateSelectedTime();

            if (SystemConfig.ProcessGroupModelBlackList == null)
                SystemConfig.ProcessGroupModelBlackList = new List<ProcessGroupModel>();
            if (SystemConfig.ProcessModelBlackList == null)
                SystemConfig.ProcessModelBlackList = new List<ProcessModel>();

            TimerProcessingLogModels = new List<TimerProcessingLogModel>();
        }

        private void UpdateSelectedTime()
        {
            totalTime = new TimeSpan(
                SelectedHour,
                SelectedMinute,
                SelectedSecond);

            if (!IsRunning)
                RemainingTime = totalTime;
        }

        private long timerTick;
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (RemainingTime.TotalSeconds <= 0)
            {
                RemainingTime = TimeSpan.Zero;
                timer.Stop();
                IsRunning = false;

                CurrnetProcessingModel.EndTime = DateTime.Now;
                TimerProcessingLogModels.Add(CurrnetProcessingModel);
                CurrnetProcessingModel = null;

                return;
            }

            RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));

            if (timerTick % 5 == 0)
            {
                Task.Run(() =>
                {
                    var processes = ProcessStatusManager.GetProcessCategoryGroupList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProcessGroupModels = processes;
                    });
                });
            }

            timerTick = timerTick > 1000000 ? 0 : timerTick + 1;
        }

        private ICommand controlTimercommand;
        public ICommand ControlTimercommand => controlTimercommand ?? (controlTimercommand = new RelayCommand(ControlTimerAction));

        private void ControlTimerAction()
        {
            if (!IsRunning)
            {
                if (remainingTime.TotalSeconds <= 0)
                    RemainingTime = totalTime;

                timer.Start();
                IsRunning = true;


                var model = new TimerProcessingLogModel();
                model.Title = SystemConfig.StatusText;
                model.LoggingDate = DateTime.Now;
                model.StartTime = DateTime.Now;

                CurrnetProcessingModel = model;
            }
            else
            {
                timer.Stop();
                IsRunning = false;
            }
        }

        private ICommand resetTimercommand;
        public ICommand ResetTimercommand => resetTimercommand ?? (resetTimercommand = new RelayCommand(ResetTimerAction));

        private void ResetTimerAction()
        {
            timer.Stop();
            IsRunning = false;
            RemainingTime = totalTime;

            if(CurrnetProcessingModel != null)
            {
                CurrnetProcessingModel.EndTime = DateTime.Now;
                TimerProcessingLogModels.Add(CurrnetProcessingModel);
                CurrnetProcessingModel = null;
            }
            

            OnPropertyChanged(nameof(TimerText));
        }

        private ICommand showDetailViewcommand;
        public ICommand ShowDetailViewcommand => showDetailViewcommand ?? (showDetailViewcommand = new RelayCommand(ShowDetailViewAction));

        private void ShowDetailViewAction()
        {
            IsShowDetailView = !IsShowDetailView;
        }

        private ICommand addBlacklistCommand;
        public ICommand AddBlacklistCommand => addBlacklistCommand ?? (addBlacklistCommand = new RelayCommand<object>(AddBlacklistAction));

        private void AddBlacklistAction(object parameter)
        {
            if (parameter is ProcessGroupModel processGroup)
            {
                // 그룹 우클릭
                var processName = processGroup.ProcessName;

                foreach (var process in processGroup.Items)
                {
                    process.IsBlocked = true;
                }
                processGroup.IsBlocked = true;

                SystemConfig.ProcessGroupModelBlackList.Add(processGroup);

                var originProcess = ProcessGroupModels.Select(x => x.Items.FirstOrDefault(x => x.ProcessName == processName));

                foreach (var item in originProcess)
                {
                    if (item != null)
                        item.IsBlocked = true;
                }
            }
            else if (parameter is ProcessModel process)
            {
                // 실제 프로세스 우클릭
                var processName = process.ProcessName;
                var pid = process.Id;

                process.IsBlocked = true;
                SystemConfig.ProcessModelBlackList.Add(process);
            }
        }

        private ICommand removeBlacklistCommand;
        public ICommand RemoveBlacklistCommand => removeBlacklistCommand ?? (removeBlacklistCommand = new RelayCommand<object>(RemoveBlacklistAction));

        private void RemoveBlacklistAction(object parameter)
        {
            if (parameter is ProcessGroupModel processGroup)
            {
                var processName = processGroup.ProcessName;
                var existProcess = SystemConfig.ProcessGroupModelBlackList.Where(x => x.ProcessName != processName).ToList();
                if (existProcess == null || existProcess.Count == 0)
                    return;

                var index = SystemConfig.ProcessGroupModelBlackList.IndexOf(existProcess.First());

                if (index != -1)
                    SystemConfig.ProcessGroupModelBlackList.RemoveAt(index);
            }
            else if (parameter is ProcessModel process)
            {
                var processName = process.ProcessName;

                var existProcess = SystemConfig.ProcessModelBlackList.Where(x => x.ProcessName != processName).ToList();
                if (existProcess == null || existProcess.Count == 0)
                    return;

                var index = SystemConfig.ProcessModelBlackList.IndexOf(existProcess.First());
                if (index != -1)
                    SystemConfig.ProcessModelBlackList.RemoveAt(index);
            }
        }

        private ICommand saveConfigCommand;
        public ICommand SaveConfigCommand => saveConfigCommand ?? (saveConfigCommand = new RelayCommand(SaveConfigAction));

        private void SaveConfigAction()
        {
            SystemConfig.Save();
        }
    }
}
