using System;
using System.Threading;

namespace AndromedaCore
{
    public interface IAction
    {
        string ActionName { get; }
        string Description { get; }
        string Category { get; }
        CancellationTokenSource CancellationToken { get; }
        event EventHandler CancellationRequest;

        void RunCommand(string rawDeviceList);
    }
}