namespace Andromeda_Actions_Core.Infrastructure
{
    public interface ILogger
    {
        void Log(string line);
        void Flush();
    }
}