namespace TailBlazer.Infrastucture
{
    using System;
    using System.Windows.Input;
    /// <summary>A command wich accepts no parameter - assumes the view model will do the work</summary>
    public class Command<T> : ICommand
    {
        #region Fields
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;
        #endregion
        #region Constructors
        public Command(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute ?? (t => true);
        }
        #endregion
        #region Events
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
        #endregion
        #region Methods
        public bool CanExecute(object parameter) => this._canExecute((T)parameter);
        public void Execute(object parameter) { this._execute((T)parameter); }
        public void Refresh() { CommandManager.InvalidateRequerySuggested(); }
        #endregion
    }
    public class Command : ICommand
    {
        #region Fields
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        #endregion
        #region Constructors
        public Command(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            this._execute = execute;
            this._canExecute = canExecute ?? (() => true);
        }
        #endregion
        #region Events
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
        #endregion
        #region Methods
        public bool CanExecute(object parameter) => this._canExecute();
        public void Execute(object parameter) { this._execute(); }
        public void Refresh() { CommandManager.InvalidateRequerySuggested(); }
        #endregion
    }
}