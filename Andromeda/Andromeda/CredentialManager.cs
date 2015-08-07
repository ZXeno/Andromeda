using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.DirectoryServices.AccountManagement;
using System.Security;
using Andromeda.Model;

namespace Andromeda
{
    public class CredentialManager
    {
        public delegate void CredentialsChanged(string domain, string user, string password);
        public static event CredentialsChanged CredentialsChangedHandler;

        private bool _isImpersonating = true;

        private CredToken _creds;
        public CredToken UserCredentials { get { return _creds; } }

        public bool IsImpersonationEnabled { get { return _isImpersonating; } }

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

        public bool DoesUserExistInActiveDirectory(string userName)
        {
            try
            {
                using (var domainContext = new PrincipalContext(ContextType.Domain, Environment.UserDomainName))
                {
                    using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, userName))
                    {
                        return foundUser != null;
                    }
                }
            }
            catch
            {
                return false;
            }


        }

        public bool IsUserLocal(string userName)
        {
            bool exists = false;
            using (var domainContext = new PrincipalContext(ContextType.Machine))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, userName))
                {
                    exists = true;
                }
            }

            return exists;
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
