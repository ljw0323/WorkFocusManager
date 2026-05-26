using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkFocusManager.ViewModels;
using Forms = System.Windows.Forms;

namespace WorkFocusManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Forms.NotifyIcon trayIcon;

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            trayIcon = CreateTrayIcon();
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
        }

        private void CatImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private Forms.NotifyIcon CreateTrayIcon()
        {
            var executablePath = Environment.ProcessPath;
            var icon = executablePath == null
                ? System.Drawing.SystemIcons.Application
                : System.Drawing.Icon.ExtractAssociatedIcon(executablePath) ?? System.Drawing.SystemIcons.Application;

            var notifyIcon = new Forms.NotifyIcon
            {
                Icon = icon,
                Text = "WorkFocusManager",
                Visible = true,
                ContextMenuStrip = new Forms.ContextMenuStrip()
            };

            notifyIcon.ContextMenuStrip.Items.Add("보이기/숨기기", null, (_, _) => Dispatcher.Invoke(ToggleWindowVisibility));
            notifyIcon.ContextMenuStrip.Items.Add("시작/일시정지", null, (_, _) => Dispatcher.Invoke(ViewModel.ToggleTimer));
            notifyIcon.ContextMenuStrip.Items.Add("리셋", null, (_, _) => Dispatcher.Invoke(ViewModel.ResetTimer));
            notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add("종료", null, (_, _) => Dispatcher.Invoke(Close));
            notifyIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ToggleWindowVisibility);

            return notifyIcon;
        }

        private void ToggleWindowVisibility()
        {
            if (IsVisible && WindowState != WindowState.Minimized)
            {
                Hide();
                return;
            }

            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }
    }
}
