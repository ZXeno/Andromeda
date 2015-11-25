using System;
using System.Collections.Generic;
using System.Windows;
using Andromeda.Model;
using Andromeda.ViewModel;
using Microsoft.Win32;

namespace Andromeda.Logic.Command
{
    public class ModifyWorkgroup : Action
    {
        private const string PeacehealthWorkgroupPath = "SOFTWARE\\PeaceHealth\\wrkgrp";

        public ModifyWorkgroup()
        {
            ActionName = "Modify Workgroup Code";
            Description = "Modfies (or creates) the workgroup code for device(s).";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);
            
            var newPrompt = new CliViewModel();
            newPrompt.OpenNewPrompt();

            Logger.Log("Opening CLI prompt.");

            try
            {
                if (newPrompt.Result)
                {
                    var newWorkgroup = newPrompt.TextBoxContents;
                    newPrompt = null;

                    foreach (var device in confirmedConnectionList)
                    {
                        var regKey = RegistryFunctions.GetRegistryKey(device, RegistryHive.LocalMachine, PeacehealthWorkgroupPath);
                        if (regKey != null)
                        {
                            regKey.SetValue("wrkgrp", newWorkgroup);
                            regKey.Close();
                            ResultConsole.AddConsoleLine(device + " workgroup set to " + newWorkgroup + ".");
                            Logger.Log(device + " workgroup set to " + newWorkgroup + ".");
                        }

                        ProgressData.OnUpdateProgressBar(1);
                    }

                    if (failedlist.Count > 0)
                    {
                        WriteToFailedLog(ActionName, failedlist);
                    }
                }
                else //if (newPrompt.WasCanceled)
                {
                    newPrompt.Dispose();
                    Logger.Log(ActionName + " was canceled.");
                    ResultConsole.AddConsoleLine(ActionName + " was canceled.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("There was an error running this command. \n " + ex.Message);
                ResultConsole.AddConsoleLine("Command failed with exception error caught: \n" + ex.Message);
            }

            if (newPrompt != null)
            {
                newPrompt.Dispose();
                newPrompt = null;
            }
        }
    }
}