using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class PingTest : Action
    {
        public PingTest()
        {
            ActionName = "Ping Test";
            Description = "Runs a ping test against the device list.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            var devlist = ParseDeviceList(rawDeviceList);
            var failedlist = new List<string>();

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (device == "")
                    {
                        continue;
                    }

                    ResultConsole.AddConsoleLine(ParseResponse(NetworkConnections.PingTest(device), device));
                }
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
                ResetCancelToken();
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }


        private string ParseResponse(PingReply hostname, string device)
        {
            string returnMsg = "";

            try
            {
                // If the ping reply isn't null...
                if (hostname != null)
                {
                    // based on our connection status, return a message
                    switch (hostname.Status)
                    {
                        case IPStatus.Success:
                            returnMsg = $"Reply from {device} with address {hostname.Address}";
                            break;
                        case IPStatus.TimedOut:
                            returnMsg = "Connection has timed out.";
                            break;
                        default:
                            returnMsg = $"Ping to {device} failed with message {NetworkConnections.GetIpStatusMessage(hostname.Status)}";
                            break;
                    }
                }
                else
                {
                    returnMsg = "Connection failed for an unknown reason. Likely, the host name is not known.";
                }
            }
            catch (PingException ex)
            {
                returnMsg = $"Connection Error: {ex.Message}";
            }
            catch (SocketException ex)
            {
                returnMsg = $"Connection Error: {ex.Message}";
            }

            return returnMsg;
        }
    }
}
