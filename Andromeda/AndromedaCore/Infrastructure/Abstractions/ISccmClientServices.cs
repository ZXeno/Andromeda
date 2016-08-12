using System.Management;

namespace AndromedaCore.Infrastructure
{
    public interface ISccmClientServices
    {
        void TriggerClientAction(string scheduleId, ManagementScope remote);
    }
}