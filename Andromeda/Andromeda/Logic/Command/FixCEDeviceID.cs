using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class FixCEDeviceID : Action
    {
        private const string DeviceIdXmlFilePath = "\\IDX\\CW\\bin\\DeviceID.XML";

        public FixCEDeviceID()
        {
            ActionName = "Fix CE DeviceID XML File";
            Description = "Repairs the DeviceID.XML file for a given device.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            foreach (string device in confirmedConnectionList)
            {
                try
                {
                    if (ValidateFileExists(device, DeviceIdXmlFilePath))
                    {
                        try
                        {
                            File.Delete(device);
                            RepairXML("\\\\" + device + "\\C$\\IDX\\CW\\bin\\DeviceID.XML", device);
                            ResultConsole.AddConsoleLine("DeviceID.XML on " + device + " has been repaired.");
                        }
                        catch (Exception ex)
                        {
                            ResultConsole.AddConsoleLine("There was a problem repairing the DeviceID.XML file.");
                            ResultConsole.AddConsoleLine(ex.Message);
                        }
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine("Unable to validate DeviceID.XML on device: " + device);
                    }
                }
                catch (Exception ex)
                {
                    ResultConsole.AddConsoleLine("An exception occurred!");
                    ResultConsole.AddConsoleLine(ex.Message);
                }

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }

        private void RepairXML(string path, string device)
        {
            File.Delete("\\\\" + device + "\\C$\\IDX\\CW\\bin\\DeviceID.XML");

            try
            {
                XmlWriterSettings _xset = new XmlWriterSettings();
                _xset.Encoding = Encoding.UTF8;
                _xset.Indent = true;

                XmlWriter _xwriter = XmlWriter.Create(path, _xset);
                _xwriter.WriteStartDocument();
                _xwriter.WriteStartElement("Device");
                _xwriter.WriteStartElement("ID");

                _xwriter.WriteString(device.ToUpper());
                
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
