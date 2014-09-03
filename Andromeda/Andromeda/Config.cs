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
            _xwriter.WriteStartElement("pingtest");
            _xwriter.WriteAttributeString("flag", pingTest.ToString());
            _xwriter.WriteEndElement();

            // Save Offline Computers
            _xwriter.WriteStartElement("saveofflinecomputers");
            _xwriter.WriteAttributeString("flag", saveOfflineComputers.ToString());
            _xwriter.WriteEndElement();

            // Save Online Computers
            _xwriter.WriteStartElement("saveonlinecomputers");
            _xwriter.WriteAttributeString("flag", saveOnlineComputers.ToString());
            _xwriter.WriteEndElement();

            // Logging Level
            _xwriter.WriteStartElement("saveonlinecomputers");
            _xwriter.WriteAttributeString("flag", loggingLevel.ToString());
            _xwriter.WriteEndElement();

            // Prompt User for Credentials
            _xwriter.WriteStartElement("promptuserforcreds");
            _xwriter.WriteAttributeString("flag", promptForCredentials.ToString());
            _xwriter.WriteEndElement();

            // Saved Credentials
            _xwriter.WriteStartElement("savedCredentails");
            _xwriter.WriteAttributeString("flag", saveCredentails.ToString());
            // Saved Credentails warnings and observed flags
            _xwriter.WriteStartElement("warningText");
            _xwriter.WriteString(warningText);
            _xwriter.WriteEndElement();
            _xwriter.WriteStartElement("warningObserved");
            _xwriter.WriteAttributeString("flag", firstWarningObserved.ToString());
            _xwriter.WriteEndElement();

            _xwriter.WriteStartElement("secondWarningText");
            _xwriter.WriteString(secondWarningText);
            _xwriter.WriteEndElement();
            _xwriter.WriteStartElement("secondWarningObserved");
            _xwriter.WriteAttributeString("flag", secondWarningObserved.ToString());
            _xwriter.WriteEndElement();

            _xwriter.WriteStartElement("thirdWarningText");
            _xwriter.WriteString(thirdWarningText);
            _xwriter.WriteEndElement();
            _xwriter.WriteStartElement("thirdWarningText");
            _xwriter.WriteAttributeString("flag", thirdWarningObserved.ToString());
            _xwriter.WriteEndElement();
            // Saved Username
            _xwriter.WriteStartElement("savedUser");
            _xwriter.WriteString(savedUserName);
            _xwriter.WriteEndElement();

            _xwriter.WriteStartElement("savedPass");
            _xwriter.WriteString(savedPass);
            _xwriter.WriteEndElement();

            // Close Saved Credentials
            _xwriter.WriteEndElement();

            // Always Dump console history on exit
            _xwriter.WriteStartElement("alwaysDumpConsoleHistory");
            _xwriter.WriteAttributeString("flag", alwaysDumpConsoleHistory.ToString());
            _xwriter.WriteEndElement();

            // Close <settings>
            _xwriter.WriteEndElement();

            // SCCM config settings
            _xwriter.WriteStartElement("sccmconfig");

            // Check Services List settings
            _xwriter.WriteStartElement("checkServicesList");
            _xwriter.WriteAttributeString("flag", checkServicesList.ToString());
            foreach (string sv in servicesList) // Fill in the services list.
            {
                _xwriter.WriteStartElement("svc");
                _xwriter.WriteString(sv);
                _xwriter.WriteEndElement();
            }
            // Close Check Services List
            _xwriter.WriteEndElement();

            // Auto Install Client
            _xwriter.WriteStartElement("autoInstallClient");
            _xwriter.WriteAttributeString("flag", autoInstallClient.ToString());
            _xwriter.WriteEndElement();

            // check enabled DCOM
            _xwriter.WriteStartElement("checkEnabledDCOM");
            _xwriter.WriteAttributeString("flag", checkEnabledDCOM.ToString());
            _xwriter.WriteEndElement();

            // check enabled remote connect
            _xwriter.WriteStartElement("checkEnabledRemoteConnect");
            _xwriter.WriteAttributeString("flag", checkEnabledRemoteConnect.ToString());
            _xwriter.WriteEndElement();

            // ccm setup path
            _xwriter.WriteStartElement("ccmSetupPath");
            _xwriter.WriteAttributeString("flag", "\"\"");
            _xwriter.WriteEndElement();

            // ccm setup parameters
            _xwriter.WriteStartElement("ccmSetupParameters");
            _xwriter.WriteAttributeString("flag","\"\"");
            _xwriter.WriteEndElement();

            // enable full uninstall
            _xwriter.WriteStartElement("enableFullUninstall");
            _xwriter.WriteAttributeString("flag", enableFullUninstall.ToString());
            _xwriter.WriteEndElement();

            // site server
            _xwriter.WriteStartElement("sccmSiteServer");
            _xwriter.WriteAttributeString("flag", "\"\"");
            _xwriter.WriteEndElement();

            // Close SCCM Config Settings
            _xwriter.WriteEndElement();
            // Close <config>
            _xwriter.WriteEndElement();

            // Close file
            _xwriter.Close();
        }

        public void UpdateConfigDocument(XmlDocument configdat)
        {
            configFileDat = configdat;
        }


    }
}
