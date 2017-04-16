using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.ViewModel;

namespace AndromedaActions.ViewModel
{
    public class DeleteOldProfilesPromptViewModel : RequestCloseViewModel
    {
        private int _dayCount = 14;
        public int DayCount => _dayCount;
        public string DayCountBoxContent
        {
            get { return _dayCount.ToString(); }
            set
            {
                var val = value;
                int num = 0;
                var valid = int.TryParse(val, out num);

                if (num < 0) { num = 0; }

                if (valid)
                {
                    _dayCount = Convert.ToInt32(num);
                }

                OnPropertyChanged("DayCountBoxContent");
                OnPropertyChanged("DayCount");
            }
        }

        private ICommand _okayCmd;
        public ICommand OkayCommand
        {
            get
            {
                if (_okayCmd == null)
                {
                    _okayCmd = new DelegateCommand(param => OkayClose(), param => true);
                }
                return _okayCmd;
            }
        }

        private ICommand _cancelCmd;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new DelegateCommand(param => CancelClose(), param => true);
                }
                return _cancelCmd;
            }
        }

        private bool _result;
        public bool Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public void OkayClose()
        {
            _result = true;
            OnRequestClose(EventArgs.Empty);
        }

        public void CancelClose()
        {
            _result = false;
            OnRequestClose(EventArgs.Empty);
        }
    }
}