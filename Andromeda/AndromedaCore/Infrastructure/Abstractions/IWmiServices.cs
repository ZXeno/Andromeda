using System.Management;
using AndromedaCore.Model;

namespace AndromedaCore.Infrastructure
{
    public interface IWmiServices
    {
        string RootNamespace { get; }
        ManagementScope ConnectToRemoteWmi(string hostname, string scope, ConnectionOptions options);
        string GetProcessReturnValueText(int retval);
        bool RepairRemoteWmi(string hostname, CredToken credentialToken);
        bool KillRemoteProcessByName(string device, string procName, ManagementScope remote);
        bool PerformRemoteUninstallByName(string device, string prodName, ManagementScope remote);
        bool PerformRemoteUninstallByProductId(string device, string prodId, ManagementScope remote);
        void ForceRebootRemoteDevice(string device, ManagementScope remote);

    }
}