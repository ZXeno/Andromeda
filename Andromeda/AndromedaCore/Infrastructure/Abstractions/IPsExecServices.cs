using AndromedaCore.Model;

namespace AndromedaCore.Infrastructure
{
    public interface IPsExecServices
    {
        void RunOnDeviceWithAuthentication(string device, string commandline, CredToken creds);
        void RunOnDeviceWithoutAuthentication(string device, string commandline);
    }
}