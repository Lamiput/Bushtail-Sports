using System;
using System.Windows.Input;

namespace Bushtail_Sports.Utils
{
    class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Predicate<object> canExecute;
        private event EventHandler CanExecuteChangedInternal;

        public RelayCommand(Action<object> _execute) : this(_execute, DefaultCanExecute) { }
        public RelayCommand(Action<object> _execute, Predicate<object> _canExecute)
        {
            execute = _execute ?? throw new ArgumentNullException("execute");
            canExecute = _canExecute ?? throw new ArgumentNullException("canExecute");
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object _parameter)
        {
            return canExecute != null && canExecute(_parameter);
        }

        public void Execute(object _parameter)
        {
            execute(_parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChangedInternal;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy()
        {
            canExecute = _ => false;
            execute = _ => { return; };
        }

        private static bool DefaultCanExecute(object _parameter)
        {
            return true;
        }
    }
}
