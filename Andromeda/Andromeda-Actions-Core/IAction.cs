using System;
using System.Threading;

namespace Andromeda_Actions_Core
{
    public interface IAction
    {
        string ActionName { get; }
        string Description { get; }
        ActionGroup Category { get; }
        CancellationTokenSource CancellationToken { get; }
        event EventHandler CancellationRequest;

        void RunCommand(string rawDeviceList);
    }
}