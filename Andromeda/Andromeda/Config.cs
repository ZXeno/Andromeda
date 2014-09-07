using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Andromeda
{
    public class Config
    {
        private bool pingTest = true;
        private bool saveOfflineComputers = true;
        private bool saveOnlineComputers = true;
        private int loggingLevel = 3;
        private bool promptForCredentials = true;
        private bool saveCredentails = false;
        private bool firstWarningObserved = false;
        private bool secondWarningObserved = false;
        private bool thirdWarningObserved = false;
        private string warningText = "--IT IS HIGHLY RECOMMENDED YOU DO NOT SAVE YOUR CREDENTAILS.-- \n Saving your credentials is a major security risk. \n Are you absolutely, completely sure you want to save credentials?";
        private string secondWarningText = "If you save your credentials, you are responsible for all actions taken under those credentials. \n This is your second warning not to save credentials.";
        private string thirdWarningText = "Okay, I get it... You're going to save credentials. \n I'm still telling you not to. It's no joke. \n You could get in trouble if they leak.";
        private string savedUserName = "Don't do it!";
        private string savedPass = "";
        private bool alwaysDumpConsoleHistory = true;
        private bool checkServicesList = true;
        private List<string> servicesList = new List<string>();
        private bool autoInstallClient = false;
        private bool checkEnabledDCOM = true;
        private bool checkEnabledRemoteConnect = false;
        private string ccmSetupDir = "";
        private string ccmSetupParameters = "";
        private bool enableFullUninstall = false;
        private string sccmSiteServer = "";

        public bool PingTest { get { return pingTest; } }
        public bool SaveOfflineComputers { get { return saveOfflineComputers; } }
        public bool SaveOnlineComputers { get { return saveOnlineComputers; } }
        public int LoggingLevel { get { return loggingLevel; } }
        public bool PromptForCredentials { get { return promptForCredentials; } }
        public bool SaveCredentials { get { return saveCredentails; } }
        public bool FirstWarningObserved { get { return firstWarningObserved; } }
        public bool SecondWarningObserved { get { return secondWarningObserved; } }
        public bool ThirdWarningObserved { get { return thirdWarningObserved; } }
        public string WarningText { get { return warningText; } }
        public string SecondWarningText { get { return secondWarningText; } }
        public string ThirdWarningText { get { return thirdWarningText; } }
        public string SavedUsername { get { return savedUserName; } }
        public string SavedPassword { get { return savedPass; } }
        public bool AlwaysDumpConsoleHistory { get { return alwaysDumpConsoleHistory; } }
        public bool CheckServicesList { get { return checkServicesList; } }
        public List<string> ServicesList { get { return servicesList; } }
        public bool AutoInstallClient { get { return autoInstallClient; } }
        public bool CheckEnabledDCOM { get { return checkEnabledDCOM; } }
        public bool CheckEnabledRemoteConnect { get { return checkEnabledRemoteConnect; } }
        public string CCMSetupDirectory { get { return ccmSetupDir; } }
        public string CCMSetupParameters { get { return ccmSetupParameters; } }
        public bool EnableFullUninstall { get { return enableFullUninstall; } }
        public string SCCMSiteServer { get { return sccmSiteServer; } }

        private XmlTextWriter _xwriter;
        private XmlDocument configFileDat;


        public Config() { }

        public Config(XmlDocument configFile)
        {

        }

        public void CreateNewConfigFile(string FilePath)
        {
            servicesList.Add("winmgmt");
            servicesList.Add("lanmanserver");
            servicesList.Add("rpcss");
            servicesList.Add("auauserv");
            servicesList.Add("bits");
            servicesList.Add("ccmexec");

            _xwriter = new XmlTextWriter(FilePath, Encoding.UTF8);
            _xwriter.Formatting = Formatting.Indented;
            _xwriter.WriteStartDocument();
            _xwriter.WriteStartElement("config");

            //Program Settings Category
            _xwriter.WriteStartElement("settings");

            // Pingtest flag
            CreateSingleAttributeElement("pingtest", "flag", pingTest.ToString());

            // Save Offline Computers
            CreateSingleAttributeElement("saveofflinecomputers", "flag", saveOfflineComputers.ToString());

            // Save Online Computers
            CreateSingleAttributeElement("saveonlinecomputers", "flag", saveOnlineComputers.ToString());

            // Logging Level
            CreateSingleAttributeElement("logginglevel", "flag", loggingLevel.ToString());

            // Prompt User for Credentials
            CreateSingleAttributeElement("promptuserforcreds", "flag", promptForCredentials.ToString());

            // Saved Credentials
            _xwriter.WriteStartElement("savedCredentails");
            _xwriter.WriteAttributeString("flag", saveCredentails.ToString());
            // Saved Credentails warnings and observed flags
            CreateUnattributedElement("warningText", warningText);
            CreateSingleAttributeElement("warningObserved", "flag", firstWarningObserved.ToString());


            CreateUnattributedElement("secondWarningText", secondWarningText);
            CreateSingleAttributeElement("secondWarningObserved", "flag", secondWarningObserved.ToString());

            CreateUnattributedElement("thirdWarningText", thirdWarningText);
            CreateSingleAttributeElement("thirdWarningObserved", "flag", thirdWarningObserved.ToString());

            // Saved Username
            CreateUnattributedElement("savedUser", savedUserName);
            CreateUnattributedElement("savedPass", savedPass);

            // Close Saved Credentials
            _xwriter.WriteEndElement();

            // Always Dump console history on exit
            CreateSingleAttributeElement("alwaysDumpConsoleHistory", "flag", alwaysDumpConsoleHistory.ToString());

            // Close <settings>
            _xwriter.WriteEndElement();

            // SCCM config settings
            _xwriter.WriteStartElement("sccmconfig");

            // Check Services List settings
            _xwriter.WriteStartElement("checkServicesList");
            _xwriter.WriteAttributeString("flag", checkServicesList.ToString());
            foreach (string sv in servicesList) // Fill in the services list.
            {
                CreateUnattributedElement("svc", sv);
            }
            // Close Check Services List
            _xwriter.WriteEndElement();

            // Auto Install Client
            CreateSingleAttributeElement("autoInstallClient", "flag", autoInstallClient.ToString());

            // check enabled DCOM
            CreateSingleAttributeElement("checkEnabledDCOM", "flag", checkEnabledDCOM.ToString());

            // check enabled remote connect
            CreateSingleAttributeElement("checkEnabledRemoteConnect", "flag", checkEnabledRemoteConnect.ToString());

            // ccm setup path
            CreateSingleAttributeElement("ccmSetupPath", "flag", "\"\"");

            // ccm setup parameters
            CreateSingleAttributeElement("ccmSetupParameters", "flag", "\"\"");

            // enable full uninstall
            CreateSingleAttributeElement("enableFullUninstall", "flag", enableFullUninstall.ToString());

            // site server
            CreateSingleAttributeElement("sccmSiteServer", "flag", "\"\"");

            // Close SCCM Config Settings
            _xwriter.WriteEndElement();
            // Close <config>
            _xwriter.WriteEndElement();

            // Close file
            _xwriter.WriteEndDocument();
            _xwriter.Close();
        }

        public void UpdateConfigDocument(XmlDocument configdat)
        {
            configFileDat = configdat;
        }

        private void CreateSingleAttributeElement(string ElementName, string AttributeString1, string AttributeString2)
        {
            _xwriter.WriteStartElement(ElementName);
            _xwriter.WriteAttributeString(AttributeString1, AttributeString2);
            _xwriter.WriteEndElement();
        }

        private void CreateUnattributedElement(string ElementName, string StringData)
        {
            _xwriter.WriteStartElement(ElementName);
            _xwriter.WriteString(StringData);
            _xwriter.WriteEndElement();
        }
    }
}
