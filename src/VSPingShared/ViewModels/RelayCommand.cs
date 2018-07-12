using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VSPing.Models;
using VSPing.Utils;

namespace VSPing.ViewModels
{
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Handles the execution of relay commands between view models
        /// </summary>
        #region Fields 
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion // Fields 
        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null) { }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute; _canExecute = canExecute;
        }
        #endregion // Constructors 
        #region ICommand Members 

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter) { _execute(parameter); }
        #endregion // ICommand Members 
    }

}
