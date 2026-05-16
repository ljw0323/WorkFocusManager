using System.Configuration;
using System.Data;
using System.Windows;
using WorkFocusManager.Configs;

namespace WorkFocusManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = SystemConfig.Load();

            SystemConfig.Instance.StatusText = config.StatusText;
            SystemConfig.Instance.Name = config.Name;
            SystemConfig.Instance.ProcessGroupModelBlackList = config.ProcessGroupModelBlackList;
            SystemConfig.Instance.ProcessModelBlackList = config.ProcessModelBlackList;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemConfig.Instance.Save();

            base.OnExit(e);
        }
    }
}