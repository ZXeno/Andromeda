using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Andromeda.Command
{
    class FixCEDeviceID : Action
    {
        private NetworkConnections netconn;

        public FixCEDeviceID()
        {
            ActionName = "Fix CE DeviceID XML File";
            Desctiption = "Repairs the DeviceID.XML file for a given device.";
            Category = ActionGroup.Other;
            netconn = new NetworkConnections();
        }

        public override void RunCommand(string a)
        {
            List<string> devlist = ParseDeviceList(a);

            foreach (string d in devlist)
            {
                try
                {
                    if (ValidateFileExists(d))
                    {
                        try
                        {
                            File.Delete(d);
                            RepairXML("\\\\" + d + "\\C$\\IDX\\CW\\bin\\DeviceID.XML", d);
                            ResultConsole.AddConsoleLine("DeviceID.XML on " + d + " has been repaired.");
                        }
                        catch (Exception ex)
                        {
                            ResultConsole.AddConsoleLine("There was a problem repairing the DeviceID.XML file.");
                            ResultConsole.AddConsoleLine(ex.Message);
                            continue;
                        }
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine("Unable to validate DeviceID.XML on device: " + d);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    ResultConsole.AddConsoleLine("An exception occurred!");
                    ResultConsole.AddConsoleLine(ex.Message);
                }
            }
        }

        private bool ValidateFileExists(string device)
        {
            try
            {
                return File.Exists("\\\\" + device + "\\C$\\IDX\\CW\\bin\\DeviceID.XML");
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception when validating the DeviceID.XML file for machine: " + device);
                ResultConsole.AddConsoleLine(ex.Message);
                return false;
            }
        }

        private void RepairXML(string path, string device)
        {
            try
            {
                XmlWriterSettings _xset = new XmlWriterSettings();
                _xset.Encoding = UTF8Encoding.UTF8;
                _xset.Indent = false;

                XmlWriter _xwriter = XmlWriter.Create(path, _xset);
                _xwriter.WriteStartDocument();
                _xwriter.WriteStartElement("Device");
                _xwriter.WriteStartElement("ID");
                _xwriter.WriteString(device);
                _xwriter.WriteEndElement();
                _xwriter.WriteEndElement();
                _xwriter.WriteEndDocument();
                _xwriter.Close();
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an error trying to repair the DeviceID.XML file on device " + device);
                ResultConsole.AddConsoleLine("Exception returned: ");
                ResultConsole.AddConsoleLine(ex.Message);
                ResultConsole.AddConsoleLine("You will need to visit the device to complete the repair.");
            }
        }
    }
}
