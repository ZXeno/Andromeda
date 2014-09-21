using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Management;

namespace Andromeda.Credentials
{
    public class CredToken
    {
        private string _usr;
        private SecureString _pw;
        private string _domain;
        private bool _canImpersonate;

        public string User { get { return _usr; } }
        public SecureString SecurePassword { get { return _pw; } }
        public string Domain { get { return _domain; } }
        public bool CanImpersonate { get { return _canImpersonate; } }

        public CredToken() { }

        public CredToken(string domain, string user, SecureString Password)
        {
            _usr = user;
            _domain = domain;
            _pw = Password;
            _canImpersonate = false;
        }

        public CredToken(string domain, string user, SecureString Password, bool canimpersonate)
        {
            _usr = user;
            _domain = domain;
            _pw = Password;
            _canImpersonate = canimpersonate;
        }

        public string GetInsecurePasswordString()
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(_pw);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public ConnectionOptions GetNonImpersonatedConnOptions()
        {
            ConnectionOptions connOps = new ConnectionOptions();

            connOps.SecurePassword = this.SecurePassword;
            connOps.Username = this.User;

            return connOps;
        }

        public ConnectionOptions GetImpersonatedConnOptions()
        {
            ConnectionOptions connOps = new ConnectionOptions();
            connOps.Impersonation = ImpersonationLevel.Impersonate;
            connOps.Authentication = AuthenticationLevel.PacketPrivacy;
            connOps.EnablePrivileges = true;
            connOps.Username = this.User;
            connOps.SecurePassword = this.SecurePassword;

            return connOps;
        }
    }
}
