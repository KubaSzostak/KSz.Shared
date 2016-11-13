using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace System
{


    public abstract class CommandBase : ObservableObject, ICommand
    {

        public CommandBase()
        {
            InitRequerySuggested();
        }

        public abstract void Execute(object parameter);
        
        private void InitRequerySuggested()
        {
            CommandManager.RequerySuggested += RequerySuggested;
        }

        protected virtual void RequerySuggested(object sender, EventArgs e)
        {
            if (_lastCanExecuteChangedValue != this.CanExecute(null))
                OnCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;
        private bool _lastCanExecuteChangedValue = true;
        public void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
            _lastCanExecuteChangedValue = this.CanExecute(null);
        }

        public virtual bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { OnPropertyChanged(ref _isEnabled, value, nameof(IsEnabled)); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { OnPropertyChanged(ref _isChecked, value, nameof(IsChecked)); }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set { OnPropertyChanged(ref _caption, value, nameof(Caption)); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { OnPropertyChanged(ref _description, value, nameof(Description)); }
        }
    }


    /// <summary>
    ///     This class allows delegating the commanding logic to methods passed as parameters,
    ///     and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    public class DelegateCommand : CommandBase
    {
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null)
        { }


        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        private readonly Action _executeMethod = null;
        private readonly Func<bool> _canExecuteMethod = null;


        public override bool CanExecute(object parameter)
        {
            var baseCan = base.CanExecute(parameter);
            if (_canExecuteMethod != null)
            {
                return _canExecuteMethod() && baseCan;
            }
            return baseCan;
        }

        public override void Execute(object parameter)
        {
            if (_executeMethod != null)
            {
                _executeMethod();
            }
        }

    }


    /// <summary>
    ///     This class allows delegating the commanding logic to methods passed as parameters,
    ///     and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    /// <typeparam name="T">Type of the parameter passed to the delegates</typeparam>
    public class DelegateCommand<T> : CommandBase, ICommand
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }


        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        private readonly Action<T> _executeMethod = null;
        private readonly Func<T, bool> _canExecuteMethod = null;


        public override bool CanExecute(object parameter)
        {
            if (_canExecuteMethod != null)
            {
                return _canExecuteMethod((T)parameter);
            }
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            if (_executeMethod != null)
            {
                _executeMethod((T)parameter);
            }
        }
    }


}
