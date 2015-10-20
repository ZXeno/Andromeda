using System.Net.NetworkInformation;

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
        public PingReply PingTest(string hostname)
        {
            PingReply pr = ping.Send(hostname, 3000);
            return pr;
        }
    }
}
