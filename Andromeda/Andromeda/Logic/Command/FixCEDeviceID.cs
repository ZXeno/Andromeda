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

        public FixCEDeviceID()
        {
            ActionName = "Fix CE DeviceID XML File";
            Description = "Repairs the DeviceID.XML file for a given device.";
            Category = ActionGroup.Other;
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

                ProgressData.OnUpdateProgressBar(1);
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
