using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace Andromeda.Credentials
{
    public class CredToken
    {
        private string _usr;
        private SecureString _pw;
        private string _domain;

        public string User { get { return _usr; } }
        public SecureString SecurePassword { get { return _pw; } }
        public string Domain { get { return _domain; } }

        public CredToken() { }

        public CredToken(string domain, string user, SecureString Password)
        {
            _usr = user;
            _domain = domain;
            _pw = Password;
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
    }
}
