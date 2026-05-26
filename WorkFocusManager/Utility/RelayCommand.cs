using System.Windows.Input;

namespace WorkFocusManager.Utility
{
    public class RelayCommand : ICommand
    {
        private readonly Func<bool>? canExecute;
        private readonly Action execute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
            => execute();

        public void OnCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool>? canExecute;
        private readonly Action<T> execute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => parameter is T value && (canExecute?.Invoke(value) ?? true);

        public void Execute(object? parameter)
        {
            if (parameter is T value)
                execute(value);
        }

        public void OnCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
