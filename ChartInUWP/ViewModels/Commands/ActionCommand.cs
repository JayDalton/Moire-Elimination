using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChartInUWP.ViewModels.Commands
{
  public class ActionCommand : ICommand
  {
    private Action Action { get; set; }

    public ActionCommand(Action command) => this.Action = command;

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) => Action();
  }
}
