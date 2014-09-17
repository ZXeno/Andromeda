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
    }
}
