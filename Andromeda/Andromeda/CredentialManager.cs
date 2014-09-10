using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace Andromeda
{
    public static class CredentialManager
    {

        public static string UserName { get; set; }

        public static string GetDomain()
        {
            return Environment.UserDomainName;
        }

        public static string GetUser()
        {
            return "null";
        }

        public static string GetPass()
        {
            return "null";
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
            catch (Exception ex)
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
                    return true;
                }
            }

            return false;
        }
    }
}
