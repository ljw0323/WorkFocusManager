using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utility;
using WorkFocusManager.Models;
using WorkFocusManager.Utility;
using WpfAnimatedGif;

namespace WorkFocusManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DispatcherTimer timer;
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
                return;
            }

            RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));

            if (timerTick % 5 == 0)
                ProcessGroupModels = ProcessStatusManager.GetProcessCategoryGroupList();

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
            OnPropertyChanged(nameof(TimerText));
        }

        private ICommand showDetailViewcommand;
        public ICommand ShowDetailViewcommand => showDetailViewcommand ?? (showDetailViewcommand = new RelayCommand(ShowDetailViewAction));

        private void ShowDetailViewAction()
        {
            IsShowDetailView = !IsShowDetailView;
        }
    }
}
