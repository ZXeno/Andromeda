using System.Security;

namespace AndromedaCore.Model
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
    }
}
