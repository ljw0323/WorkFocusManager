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

                if (value.TotalSeconds <= 0)
                    ProgressAngle = 360;
                else
                    ProgressAngle = value.TotalSeconds / totalTime.TotalSeconds * 359.999;
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

        private double progressAngle;
        public double ProgressAngle
        {
            get => progressAngle;
            set => Set(ref progressAngle, value);
        }

        private string timerText;
        public string TimerText
        {
            get => timerText;
            set => Set(ref timerText, value);
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
        }

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

            if (totalTime.TotalSeconds <= 0)
                ProgressAngle = 0;
            else
                ProgressAngle = RemainingTime.TotalSeconds / totalTime.TotalSeconds * 359.999;
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
