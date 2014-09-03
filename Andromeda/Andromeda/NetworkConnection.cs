using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Text;

namespace Andromeda
{
    public class NetworkConnection
    {
        private Ping ping;
        private PingOptions pingOptions;

        public NetworkConnection()
        {
            ping = new Ping();
            pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
        }

        public void PingTest(string hostname)
        {
            string returnMsg = "";
            PingReply pr = ping.Send(hostname);
            try { }
            catch (PingException e)
            {

            }
            catch (SocketException e)
            {
                
            }

            
        }

        public void PingTest(List<string> hostnames)
        {
            try { }
            catch (PingException e)
            {

            }
            catch (SocketException e)
            {

            }
        }

        public void PingTest(string[] hostnamearray)
        {
            try { }
            catch (PingException e)
            {

            }
            catch (SocketException e)
            {

            }
        }

        public bool HasConnection()
        {
            return false;
        }
    }
}
