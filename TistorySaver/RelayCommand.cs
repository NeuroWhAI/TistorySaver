using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TistorySaver
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> m_action;
        private bool m_isEnabled;

        public RelayCommand(Action<object> action, bool isEnabled = true)
        {
            m_action = action;
            IsEnabled = isEnabled;
        }

        public RelayCommand(Action action, bool isEnabled = true)
            : this((_) => action(), isEnabled)
        {

        }

        public bool IsEnabled
        {
            get => m_isEnabled;
            set
            {
                if (value != m_isEnabled)
                {
                    m_isEnabled = value;

                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return m_isEnabled;
        }

        public void Execute(object parameter)
        {
            m_action(parameter);
        }
    }
}
