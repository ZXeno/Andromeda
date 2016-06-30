using System;

namespace Andromeda_Actions_Core.Infrastructure
{
    public interface ILoggerService
    {
        void LogMessage(string msg);
        void LogWarning(string msg, Exception e);
        void LogError(string msg, Exception e);
    }
}