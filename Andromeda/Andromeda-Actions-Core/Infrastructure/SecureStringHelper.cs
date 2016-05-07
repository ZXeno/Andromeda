using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Andromeda_Actions_Core.Infrastructure
{
    public static class SecureStringHelper
    {
        public static SecureString BuildSecureString(string insecureString)
        {
            SecureString secureStr = new SecureString();

            if (insecureString.Length > 0)
            {
                foreach (var c in insecureString.ToCharArray()) secureStr.AppendChar(c);
            }

            return secureStr;
        }

        public static string GetInsecureString(SecureString secureString)
        {
            if (secureString == null) { return ""; }

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