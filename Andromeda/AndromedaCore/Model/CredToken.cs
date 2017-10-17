using System;
using System.Security;

namespace AndromedaCore.Model
{
    public class CredToken : IDisposable
    {
        public string User { get; }
        public SecureString SecurePassword { get; }
        public string Domain { get; }
        public bool CanImpersonate { get; }

        public CredToken() { }

        public CredToken(string domain, string user, SecureString password)
        {
            User = user;
            Domain = domain;
            SecurePassword = password;
            CanImpersonate = false;
        }

        public CredToken(string domain, string user, SecureString password, bool canimpersonate)
        {
            User = user;
            Domain = domain;
            SecurePassword = password;
            CanImpersonate = canimpersonate;
        }

        public void Dispose()
        {
            SecurePassword?.Dispose();
        }
    }
}
