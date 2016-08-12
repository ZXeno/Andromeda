using System.Management;

namespace Andromeda_Actions_Core.Infrastructure
{
    public interface ISccmClientServices
    {
        void TriggerClientAction(string scheduleId, ManagementScope remote);
    }
}