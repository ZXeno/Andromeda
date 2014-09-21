using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromeda
{
    public class Program
    {
        private CredentialManager _credman;

        public CredentialManager CredentialManager { get { return _credman; } }

        public Program()
        {
            _credman = new CredentialManager();

        }

    }
}
