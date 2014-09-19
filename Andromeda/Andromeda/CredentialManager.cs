using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.DirectoryServices.AccountManagement;
using System.Security;
using System.Runtime.InteropServices;

namespace Andromeda
{
    public static class CredentialManager
    {
        private static bool _isImpersonating = true;

        private static string _uName = "";
        private static SecureString _pWord = new SecureString();

        public static string UserName { get { return _uName; } }
        public static SecureString Password { get { return _pWord; } }
        public static string Domain { get { return Environment.UserDomainName; } }
        public static bool IsImpersonationEnabled { get { return _isImpersonating; } }

        public static void SetCustomCredentials(string user, SecureString pass)
        {
            _uName = user;
            _pWord = pass;
        }

        public static string ExtractSecureString(SecureString secString)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static SecureString BuildSecureString(string strPassword)
        {
            SecureString secureStr = new SecureString();
            if (strPassword.Length > 0)
                {
                    foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
                }
            return secureStr;
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

        public static ConnectionOptions GetImpersonatedConnOptions()
        {
            ConnectionOptions connOps = new ConnectionOptions();
            connOps.Impersonation = ImpersonationLevel.Impersonate;
            connOps.EnablePrivileges = true;
            connOps.Authentication = AuthenticationLevel.PacketPrivacy;

            return connOps;
        }
    }
}
