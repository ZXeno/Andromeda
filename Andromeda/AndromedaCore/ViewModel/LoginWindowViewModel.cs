using System;
using System.Security;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;

namespace AndromedaCore.ViewModel
{
    public class LoginWindowViewModel : RequestCloseViewModel
    {
        private string _domain;
        public string Domain
        {
            get => _domain;
            set
            {
                _domain = value;
                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                OnPropertyChanged("LoginCommand");
            }
        }

        private SecureString _passwordContainer;
        public string Password
        {
            get => SecureStringHelper.GetInsecureString(_passwordContainer);
            set
            {
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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

            if (Username.Contains("\\"))
            {
                var userdomainsplit = Username.Split('\\');
                Domain = userdomainsplit[0];
                Username = userdomainsplit[1];
            }

            try
            {
                successOnValidate = CredentialManager.ValidateCredentials(Domain, Username, Password);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                exception = true;
            }
            

            if (successOnValidate)
            {
                OnRequestClose(EventArgs.Empty);
            }
            else if (!exception)
            {
                ErrorMessage = "Username or password incorrect.";
            }
        }

        private void CancelExecute()
        {
            WasCanceled = true;
            OnRequestClose(EventArgs.Empty);
        }
    }
}