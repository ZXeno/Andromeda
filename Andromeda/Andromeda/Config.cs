using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Andromeda
{
    class Config
    {
        private bool pingTest = true;
        private bool saveOfflineComputers = true;
        private bool saveOnlineComputers = true;
        private int loggingLevel = 1;
        private bool promptForCredentials = true;
        private bool saveCredentails = false;
        private bool firstWarningObserved = false;
        private bool secondWarningObserved = false;
        private bool thirdWarningObserved = false;
        private string savedUserName = "Don't do it!";
        private string savedPass = "";
        private bool checkServicesList = true;
        private List<string> servicesList = new List<string>();
        private bool autoInstallClient = false;
        private bool checkEnabledDCOM = true;
        private bool checkEnabledRemoteConnect = false;
        private string ccmSetupDir = "";
        private string ccmSetupParameters = "";
        private bool enableFullUninstall = false;
        private string sccmSiteServer = "";

        private XmlTextWriter _xwriter;


        public Config() { }

        public Config(XmlDocument configFile)
        {

        }

        public void CreateNewConfigFile()
        {
            
        }

        public void UpdateConfigDocument(XmlDocument configdat)
        {

        }
    }
}
