using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core.Infrastructure
{
    public interface IPsExecServices
    {
        void RunOnDeviceWithAuthentication(string device, string commandline, CredToken creds);
        void RunOnDeviceWithoutAuthentication(string device, string commandline);
    }
}