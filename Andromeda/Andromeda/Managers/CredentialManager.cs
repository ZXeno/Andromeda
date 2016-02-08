using System;
using System.DirectoryServices.AccountManagement;
using System.Security;
using Andromeda.Model;

namespace Andromeda
{
    public class CredentialManager
    {
        public delegate void CredentialsChanged(string domain, string user, string password);
        public static event CredentialsChanged CredentialsChangedHandler;

        private CredToken _creds;
        public CredToken UserCredentials { get { return _creds; } }
        public bool CredentialsAreValid { get; set; }

        public CredentialManager()
        {
            CredentialsChangedHandler += UpdateCredentials;
            _creds = null;
        }

        public void SetCredentials(string domain, string user, SecureString pass)
        {
            _creds = new CredToken(domain, user, pass);
        }

        public SecureString BuildSecureString(string strPassword)
        {
            SecureString secureStr = new SecureString();
            
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }

            return secureStr;
        }
        
        public bool ValidateCredentials(string domain, string user, string pass)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
            {
                // validate the credentials
                return context.ValidateCredentials(user, pass);
            }
        }

        private void UpdateCredentials(string domain, string user, string pass)
        {
            _creds = new CredToken(domain, user, BuildSecureString(pass));
        }

        public static void OnCredentialsChanged(string domain, string user, string pass)
        {
            if (CredentialsChangedHandler != null)
            {
                CredentialsChangedHandler(domain, user, pass);
            }
        }
    }
}
