using System;
using System.Management;
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
            if (hostname == "" || hostname == null)
            {
                return "No host was specified, please check the device list and try again.";
            }
            
            try 
            {
                PingReply pr = ping.Send(hostname);
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

        public ManagementScope ConnectToRemoteWMI(string hostname, string scope)
        {
            ManagementScope wmiscope = new ManagementScope("\\\\" + hostname + scope);
            wmiscope.Connect();
            return wmiscope;
        }

        public bool CheckWMIAccessible(string hostname, string scope)
        {
            ManagementScope wmiscope = ConnectToRemoteWMI(hostname, scope);
            if (wmiscope.IsConnected) { ResultConsole.AddConsoleLine("Connected to WMI scope " + wmiscope.Path); }
            else { ResultConsole.AddConsoleLine("Connection to WMI scope " + wmiscope.Path + " failed."); }
            return wmiscope.IsConnected;
        }

        public ManagementScope ConnectToSCCMscope(string hostname)
        {
            try
            {
                ManagementScope ccmscope = new ManagementScope("\\\\" + hostname + "root\\ccm");
                ccmscope.Connect();
                return ccmscope;
            }
            catch (ManagementException e)
            {
                ResultConsole.AddConsoleLine("Failed to connect.");
                ResultConsole.AddConsoleLine(e.Message);
                throw;
            }
        }


    }
}
