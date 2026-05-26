using System.Configuration;
using System.Data;
using System.Windows;
using WorkFocusManager.Configs;

namespace WorkFocusManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SystemConfig.Instance.Save();

            base.OnExit(e);
        }
    }
}
