namespace Andromeda_Actions_Core
{
    public interface IAction
    {
        string ActionName { get; }
        string Description { get; }
        ActionGroup Category { get; }

        void RunCommand(string rawDeviceList);
    }
}