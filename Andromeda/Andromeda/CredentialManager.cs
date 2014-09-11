using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.DirectoryServices.AccountManagement;

namespace Andromeda
{
    public static class CredentialManager
    {
        private static bool _isImpersonating = true;

        private static string _uName = "";
        private static string _pWord = "";

        public static string UserName { get { return _uName; } }
        public static string Password { get { return _pWord; } }
        public static string Domain { get { return Environment.UserDomainName; } }
        public static bool IsImpersonationEnabled { get { return _isImpersonating; } }

        public static void SetUser(string uName)
        {
            _uName = uName;
        }

        public static void SetPass(string pWord)
        {
            _pWord = pWord;
        }

        

        public static bool DoesUserExistInActiveDirectory(string userName)
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

        public static bool IsUserLocal(string userName)
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
    }
}
