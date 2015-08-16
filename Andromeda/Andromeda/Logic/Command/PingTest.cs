using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Andromeda.Model;

namespace Andromeda.Command
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

        public override void RunCommand(string a)
        {
            var devlist = ParseDeviceList(a);
            var successful = GetPingableDevices.GetDevices(devlist);

            foreach (string d in successful)
            {
                if (d == "") { continue; }

                ResultConsole.AddConsoleLine(ParseResponse(netConn.PingTest(d), d));
                ProgressData.OnUpdateProgressBar(1);
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
