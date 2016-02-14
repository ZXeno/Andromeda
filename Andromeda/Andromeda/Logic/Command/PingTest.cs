using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class PingTest : Action
    {
        public PingTest()
        {
            ActionName = "Ping Test";
            Description = "Runs a ping test against the device list.";
            Category = ActionGroup.Other;
            netConn=new NetworkConnections();
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            foreach (var device in confirmedConnectionList)
            {
                if (device == "")
                {
                    continue;
                }

                ResultConsole.AddConsoleLine(ParseResponse(netConn.PingTest(device), device));
            }

            if (failedlist.Count > 0)
            {
                Logger.WriteToFailedLog(ActionName, failedlist);
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
                            returnMsg = string.Format("Reply from {0} with address {1}", device, hostname.Address);
                            break;
                        case IPStatus.TimedOut:
                            returnMsg = "Connection has timed out.";
                            break;
                        default:
                            returnMsg = string.Format("Ping to {0} failed: {1}", device, hostname.Status.ToString());
                            break;
                    }
                }
                else
                {
                    // if it isn't null, but fails anyway, I'm not exactly certain why we would have an error.
                    returnMsg = "Connection failed for an unknown reason.";
                }
            }
            catch (PingException ex)
            {
                returnMsg = string.Format("Connection Error: {0}", ex.Message);
            }
            catch (SocketException ex)
            {
                returnMsg = string.Format("Connection Error: {0}", ex.Message);
            }
            
            return returnMsg;
        }
    }
}
