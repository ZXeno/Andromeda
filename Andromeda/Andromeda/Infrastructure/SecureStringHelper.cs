using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Andromeda.Infrastructure
{
    public static class SecureStringHelper
    {
        public static SecureString BuildSecureString(string strPassword)
        {
            SecureString secureStr = new SecureString();

            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }

            return secureStr;
        }

        public static string GetInsecureString(SecureString secureString)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}