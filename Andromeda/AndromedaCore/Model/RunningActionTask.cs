using System.Threading.Tasks;

namespace AndromedaCore.Model
{
    public class RunningActionTask
    {
        public Task ThisActionsTask { get; set; }
        public IAction RunningAction { get; set; }
        public string RawDeviceListString { get; set; }
        public string RunningActionName { get; set; }
        public int ThreadId { get; set; }
    }
}