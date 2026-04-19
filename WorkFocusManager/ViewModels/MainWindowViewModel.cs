using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utility;
using WorkFocusManager.Models;
using WorkFocusManager.Utility;

namespace WorkFocusManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DispatcherTimer timer;
        private TimeSpan totalTime;
        private bool isRunning;

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

        public bool IsRunning
        {
            get => isRunning;
            set => Set(ref isRunning, value);
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

        private List<ProcessModel> processList;
        public List<ProcessModel> ProcessList
        {
            get => processList;
            set => Set(ref processList, value);
        }

        public MainWindowViewModel()
        {
            totalTime = TimeSpan.FromMinutes(1);
            RemainingTime = totalTime;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            ProcessList = ProcessStatusManager.GetProcessList();
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

        private ICommand startTimercommand;
        public ICommand StartTimercommand => startTimercommand ?? (startTimercommand = new RelayCommand(StartTimerAction));

        private void StartTimerAction()
        {
            if (remainingTime.TotalSeconds <= 0)
                RemainingTime = totalTime;


            timer.Start();
            IsRunning = true;
        }

        private ICommand stopTimercommand;
        public ICommand StopTimercommand => stopTimercommand ?? (stopTimercommand = new RelayCommand(StopTimerAction));

        private void StopTimerAction()
        {
            timer.Stop();
            IsRunning = false;
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
    }
}
