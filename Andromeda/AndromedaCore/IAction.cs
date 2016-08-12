using System;
using System.Threading;

namespace AndromedaCore
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