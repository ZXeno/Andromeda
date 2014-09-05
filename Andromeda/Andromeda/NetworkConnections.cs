using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Text;

namespace Andromeda
{
    public class NetworkConnections
    {
        private Ping ping;
        private PingOptions pingOptions;

        // Constructor
        public NetworkConnections()
        {
            ping = new Ping();
            pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
        }

        // Ping test for single machine.
        public string PingTest(string hostname)
        {
            string returnMsg = "";
            PingReply pr = ping.Send(hostname);
            try 
            {
                // If the ping reply isn't null...
                if (pr != null)
                {
                    // based on our connection status, return a message
                    switch (pr.Status)
                    {
                        case IPStatus.Success:
                            returnMsg = string.Format("Reply from {0} with address {1}", hostname, pr.Address);
                            break;
                        case IPStatus.TimedOut:
                            returnMsg = "Connection has timed out.";
                            break;
                        default:
                            returnMsg = string.Format("Ping to {0} failed: {1}", hostname, pr.Status.ToString());
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

        // Return dictionary of results based on list of computers from a List<string> of hostnames.
        public Dictionary<string, string> PingTest(List<string> hostnames)
        {
            Dictionary<string,string> returnMsgs = new Dictionary<string,string>(); ;
            foreach (string host in hostnames)
            {
                returnMsgs.Add(host, PingTest(host));
            }

            return returnMsgs;
        }

        // Return dictionary of results based on a list of computers from a string array of hostnames.
        public Dictionary<string, string> PingTest(string[] hostnamearray)
        {
            Dictionary<string, string> returnMsgs = new Dictionary<string, string>();

            for(int i = 0; i <= hostnamearray.Length - 1; i++)
            {
                returnMsgs.Add(hostnamearray[i], PingTest(hostnamearray[i]));
            }

            return returnMsgs;
        }
    }
}
