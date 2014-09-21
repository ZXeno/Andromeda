using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Andromeda.Command
{
    public class PingTest : Action
    {
        NetworkConnections netConn;

        public PingTest()
        {
            ActionName = "Ping Test";
            Desctiption = "Runs a ping test against the device list.";
            Category = ActionGroup.Other;
            netConn = new NetworkConnections();
        }

        public override void RunCommand(string host)
        {
            foreach (string d in ParseDeviceList(host))
            {
                if (d == "") { continue; }
                ResultConsole.AddConsoleLine(ParseResponse(netConn.PingTest(d), d));
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
