using System;
using System.Threading;

namespace AndromedaCore
{
    public interface IAction
    {
        CancellationTokenSource CancellationToken { get; }
        event EventHandler CancellationRequest;

        string ActionName { get; }
        string Description { get; }
        string Category { get; }
        bool HasUserInterfaceElement { get; }
        System.Action UiCallback { get; }

        void OpenUserInterfaceElement(string rawDeviceList);
        void RunCommand(string rawDeviceList);
    }
}