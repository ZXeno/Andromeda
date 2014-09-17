using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Windows;

namespace Andromeda
{
    public class Config
    {
        private bool pingTest = true;
        private bool saveOfflineComputers = true;
        private bool saveOnlineComputers = true;
        private int loggingLevel = 3;
        //private bool promptForCredentials = true;
        private bool alwaysDumpConsoleHistory = true;
        private bool checkServicesList = true;
        private List<string> servicesList = new List<string>();
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
        //public bool PromptForCredentials { get { return promptForCredentials; } }
        public bool AlwaysDumpConsoleHistory { get { return alwaysDumpConsoleHistory; } }
        public bool CheckServicesList { get { return checkServicesList; } }
        public List<string> ServicesList { get { return servicesList; } }
        public bool CheckEnabledDCOM { get { return checkEnabledDCOM; } }
        public bool CheckEnabledRemoteConnect { get { return checkEnabledRemoteConnect; } }
        public string CCMSetupDirectory { get { return ccmSetupDir; } }
        public string CCMSetupParameters { get { return ccmSetupParameters; } }
        public bool EnableFullUninstall { get { return enableFullUninstall; } }
        public string SCCMSiteServer { get { return sccmSiteServer; } }

        private XmlWriter _xwriter;
        private XmlDocument configFileDat;

        public Config() { }

        public Config(string FilePath) 
        {
            CreateNewConfigFile(FilePath);
        }

        public Config(XmlDocument configFile)
        {
            try
            {
                configFileDat = configFile;

                // "config" node

                pingTest = StringToBool(configFileDat.SelectSingleNode("config/settings/pingtest").InnerText);
                saveOfflineComputers = StringToBool(configFileDat.SelectSingleNode("config/settings/saveofflinecomputers").InnerText);
                saveOnlineComputers = StringToBool(configFileDat.SelectSingleNode("config/settings/saveonlinecomputers").InnerText);
                loggingLevel = Convert.ToInt32(configFileDat.SelectSingleNode("config/settings/logginglevel").InnerText);
                alwaysDumpConsoleHistory = StringToBool(configFileDat.SelectSingleNode("config/settings/alwaysDumpConsoleHistory").InnerText);

                // "sccmconfig" node
                checkServicesList = StringToBool(configFileDat.SelectSingleNode("config/sccmconfig/checkServicesList/bool").InnerText);
                servicesList.Clear();
                foreach (XmlNode node in configFileDat.SelectSingleNode("config/sccmconfig/checkServicesList"))
                {
                    if (node.Name == "svc")
                    {
                        servicesList.Add(node.InnerText);
                    }
                    
                }

                checkEnabledDCOM = StringToBool(configFileDat.SelectSingleNode("config/sccmconfig/checkEnabledDCOM").InnerText);
                checkEnabledRemoteConnect = StringToBool(configFileDat.SelectSingleNode("config/sccmconfig/checkEnabledRemoteConnect").InnerText);
                ccmSetupDir = configFileDat.SelectSingleNode("config/sccmconfig/ccmSetupPath").InnerText;
                ccmSetupParameters = configFileDat.SelectSingleNode("config/sccmconfig/ccmSetupParameters").InnerText;
                enableFullUninstall = StringToBool(configFileDat.SelectSingleNode("config/sccmconfig/enableFullUninstall").InnerText);
                sccmSiteServer = configFileDat.SelectSingleNode("config/sccmconfig/sccmSiteServer").InnerText;

                ResultConsole.AddConsoleLine("Config file loaded.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uh oh! \n\n The config file failed to load with error message:\n" + ex.Message);
            }

        }
        #region create new config file
        public void CreateNewConfigFile(string FilePath)
        {
            ResultConsole.AddConsoleLine("Generating new config file...");

            servicesList.Add("winmgmt");
            servicesList.Add("lanmanserver");
            servicesList.Add("rpcss");
            servicesList.Add("auauserv");
            servicesList.Add("bits");
            servicesList.Add("ccmexec");

            #region Document Creation
            try
            {
                XmlWriterSettings _xsets = new XmlWriterSettings();
                _xsets.Encoding = UTF8Encoding.UTF8;
                _xsets.Indent = true;

                _xwriter = XmlWriter.Create(FilePath, _xsets);
                
                _xwriter.WriteStartDocument();
                _xwriter.WriteStartElement("config");

                //Program Settings Category
                _xwriter.WriteStartElement("settings");

                // Pingtest flag
                CreateUnattributedElement("pingtest", pingTest.ToString());

                // Save Offline Computers
                CreateUnattributedElement("saveofflinecomputers", saveOfflineComputers.ToString());

                // Save Online Computers
                CreateUnattributedElement("saveonlinecomputers", saveOnlineComputers.ToString());

                // Logging Level
                CreateUnattributedElement("logginglevel", loggingLevel.ToString());

                /*
                // Prompt User for Credentials
                CreateUnattributedElement("promptuserforcreds", promptForCredentials.ToString());
                 */

                // Always Dump console history on exit
                CreateUnattributedElement("alwaysDumpConsoleHistory", alwaysDumpConsoleHistory.ToString());

                // Close <settings>
                _xwriter.WriteEndElement();

                // SCCM config settings
                _xwriter.WriteStartElement("sccmconfig");

                // Check Services List settings
                _xwriter.WriteStartElement("checkServicesList");
                CreateUnattributedElement("bool", checkServicesList.ToString());
                foreach (string sv in servicesList) // Fill in the services list.
                {
                    CreateUnattributedElement("svc", sv);
                }
                // Close Check Services List
                _xwriter.WriteEndElement();

                // check enabled DCOM
                CreateUnattributedElement("checkEnabledDCOM", checkEnabledDCOM.ToString());

                // check enabled remote connect
                CreateUnattributedElement("checkEnabledRemoteConnect", checkEnabledRemoteConnect.ToString());

                // ccm setup path
                CreateUnattributedElement("ccmSetupPath", "\"\"");

                // ccm setup parameters
                CreateUnattributedElement("ccmSetupParameters", "\"\"");

                // enable full uninstall
                CreateUnattributedElement("enableFullUninstall", enableFullUninstall.ToString());

                // site server
                CreateUnattributedElement("sccmSiteServer", "\"\"");

                // Close SCCM Config Settings
                _xwriter.WriteEndElement();
                // Close <config>
                _xwriter.WriteEndElement();

                // Close file
                _xwriter.WriteEndDocument();
                _xwriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion

        }
        #endregion

        public void UpdateConfigDocument(XmlDocument configdat, string path)
        {
            configFileDat = configdat;
            configFileDat.Save(path);
            ResultConsole.AddConsoleLine("Config file updated.");
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

        private bool StringToBool(string tfval)
        {
            if (tfval == "True" || tfval == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
