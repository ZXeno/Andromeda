using System;
using System.Security;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.ViewModel;

namespace Andromeda.ViewModel
{
    public class LoginWindowViewModel : ViewModelBase
    {
        private CredentialManager _credManager => CredentialManager.Instance;

        public System.Action SuccessAction { get; set; }
        public System.Action CancelAction { get; set; }

        private string _domain;
        public string Domain
        {
            get => _domain;
            set
            {
                _domain = value;
                OnPropertyChanged("Domain");
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged("Username");
                OnPropertyChanged("LoginCommand");
            }
        }

        private SecureString _passwordContainer;
        public string Password
        {
            get => SecureStringHelper.GetInsecureString(_passwordContainer);
            set
            {
                OnPropertyChanged("Password");
                _passwordContainer = SecureStringHelper.BuildSecureString(value);
            }
        }

        private bool _canceled;
        public bool WasCanceled
        {
            get => _canceled;
            set
            {
                _canceled = value;
                OnPropertyChanged("WasCanceled");
            }
        }

        public ICommand LoginCommand
        {
            get { return new DelegateCommand(param => LoginExecute(), pb => CanLoginExecute()); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(param => CancelExecute(), pb => true); }
        }

        private string _errMsg;
        public string ErrorMessage
        {
            get => _errMsg;
            set
            {
                _errMsg = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        private bool CanLoginExecute()
        {
            return !string.IsNullOrEmpty(_username);
        }

        public LoginWindowViewModel()
        {
            Domain = Environment.UserDomainName;
        }
        
        private void LoginExecute()
        {
            if (!CanLoginExecute()) return;

            bool successOnValidate = false;
            bool exception = false;

            try
            {
                successOnValidate = _credManager.ValidateCredentials(Domain, Username, Password);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                exception = true;
            }
            

            if (successOnValidate)
            {
                _credManager.SetCredentials(Domain, Username, _credManager.BuildSecureString(Password));
                _credManager.CredentialsAreValid = true;
                SuccessAction.Invoke();
            }
            else if (!exception)
            {
                ErrorMessage = "Username or password incorrect.";
            }
        }

        private void CancelExecute()
        {
            WasCanceled = true;
            CancelAction.Invoke();
        }
    }
}