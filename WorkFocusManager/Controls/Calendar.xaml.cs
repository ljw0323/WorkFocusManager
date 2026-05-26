using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WorkFocusManager.Controls
{
    /// <summary>
    /// Calendar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Calendar : UserControl
    {
        public Calendar()
        {
            InitializeComponent();
        }

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(Calendar),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Dictionary<DateTime, TimeSpan> HighlightedDurations
        {
            get => (Dictionary<DateTime, TimeSpan>)GetValue(HighlightedDurationsProperty);
            set => SetValue(HighlightedDurationsProperty, value);
        }

        public static readonly DependencyProperty HighlightedDurationsProperty =
            DependencyProperty.Register(
                nameof(HighlightedDurations),
                typeof(Dictionary<DateTime, TimeSpan>),
                typeof(Calendar),
                new PropertyMetadata(
                    new Dictionary<DateTime, TimeSpan>(),
                    OnHighlightedDurationsChanged));

        private static void OnHighlightedDurationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Calendar calendar)
                calendar.ApplyHighlights();
        }

        private void ApplyHighlights()
        {
            Dispatcher.BeginInvoke(() =>
            {
                var dayButtons = FindVisualChildren<CalendarDayButton>(this);
                var maxTicks = HighlightedDurations.Count == 0
                    ? 0
                    : HighlightedDurations.Values.Max(x => x.Ticks);

                foreach (var button in dayButtons)
                {
                    if (button.DataContext is not DateTime date)
                        continue;

                    if (!HighlightedDurations.TryGetValue(date.Date, out var duration) || maxTicks == 0)
                    {
                        button.ClearValue(BackgroundProperty);
                        continue;
                    }

                    var ratio = Math.Clamp(duration.Ticks / (double)maxTicks, 0.25, 1);
                    button.Background = new SolidColorBrush(Color.FromArgb(
                        (byte)(70 + 100 * ratio),
                        107,
                        227,
                        139));
                }
            });
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    yield return typedChild;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }
    }
}
