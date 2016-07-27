using AndromedaCore.Managers;
using AndromedaCore.ViewModel;

namespace Andromeda.ViewModel
{
    public class ResultConsoleViewModel : ViewModelBase
    {
        private string _consoleString;
        public string ConsoleString
        {
            get { return _consoleString; }
            set
            {
                _consoleString = value;
                OnPropertyChanged("ConsoleString");
            }
        }

        public ResultConsoleViewModel()
        {
            ResultConsole.ConsoleChange += UpdateConsoleData;
            ResultConsole.Instance.OnConsoleChange(ResultConsole.Instance.ConsoleString);
        }

        public void UpdateConsoleData(string updateString)
        {
            ConsoleString = updateString;
        }
    }
}