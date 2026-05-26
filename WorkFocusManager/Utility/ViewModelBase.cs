using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Utility
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        [NotMapped]
        public bool IsNotifyPropertyChanged { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected bool Set<T>(ref T storage, T value, Action<T> changedAction, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            changedAction?.Invoke(value);

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (!IsNotifyPropertyChanged || propertyName == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
