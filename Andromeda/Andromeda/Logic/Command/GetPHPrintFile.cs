using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class GetPHPrintFile : Action
    {
        private readonly string _destinationDirectory;

        public GetPHPrintFile()
        {
            ActionName = "Get PHPrint File";
            Description = "Retreives the PHPrint file from the given device and copies it to results directory.";
            Category = ActionGroup.Other;

            _destinationDirectory = Config.ResultsDirectory + "\\" + "GetPHPrints\\";
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            ValidateDestinationExists();

            foreach (string device in confirmedConnectionList)
            {
                try
                {
                    if (ValidateFileExists(device))
                    {
                        try
                        {
                            if (CopyPHPrintFile(device))
                            {
                                ResultConsole.AddConsoleLine("Copied PHPrint from " + device + " to " + _destinationDirectory + "\\" + device + " - phprint.txt");
                            }
                        }
                        catch (Exception ex)
                        {
                            ResultConsole.AddConsoleLine("There was a problem copying the PHprint file.");
                            ResultConsole.AddConsoleLine(ex.Message);
                            continue;
                        }
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine("Unable to validate phprint file on device: " + device);
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

            if (Directory.Exists(_destinationDirectory))
            {
                Process.Start("explorer.exe", _destinationDirectory);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }

        private bool CopyPHPrintFile(string device)
        {
            try
            {
                if (File.Exists(_destinationDirectory + "\\" + device + " - phprint.txt"))
                {
                    File.Delete(_destinationDirectory + "\\" + device + " - phprint.txt");
                    Logger.Log("Deleting file " + _destinationDirectory + "\\" + device + " - phprint.txt");
                }

                File.Copy("\\\\" + device + "\\C$\\Windows\\PHPrint.txt", _destinationDirectory + "\\" + device + " - phprint.txt");
                Logger.Log("Copying file: " + device + " - phprint.txt");
                return true;
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception while copying the phprint file from " + device + ":");
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log("There was an exception while copying the phprint file from " + device + ". " + ex.InnerException);
                return false;
            }
            
        }

        private void ValidateDestinationExists()
        {
            try
            {
                if (!Directory.Exists(_destinationDirectory))
                {
                    Directory.CreateDirectory(_destinationDirectory);
                    Logger.Log("Creating directory " + _destinationDirectory);
                }
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception when validating or creating the destination folder.");
                ResultConsole.AddConsoleLine(ex.Message);
                Logger.Log("There was an exception when validating or creating the destination folder." + ex.InnerException);
            }
        }

        private bool ValidateFileExists(string device)
        {
            try
            {
                return File.Exists("\\\\" + device + "\\C$\\Windows\\PHPrint.txt");
            }
            catch (Exception ex)
            {
                ResultConsole.AddConsoleLine("There was an exception when validating the PHprint file for machine: " + device);
                ResultConsole.AddConsoleLine(ex.Message);
                return false;
            }
        }
    }
}