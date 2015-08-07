using System;
using System.Security;
using System.Runtime.InteropServices;

namespace Andromeda.Model
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

        public SecureString BuildSecureString(string strPassword)
        {
            SecureString secureStr = new SecureString();

            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }

            return secureStr;
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
