namespace Andromeda
{
    public interface IConfiguration
    {
        string DataFilePath { get; set; }
        bool SaveOfflineComputers { get; set; }
        bool SaveOnlineComputers { get; set; }
        bool AlwaysDumpConsoleHistory { get; set; }
        string ResultsDirectory { get; set; }
        string ComponentDirectory { get; set; }
        string FailedConnectListFile { get; set; }
        string SuccessfulConnectionListFile { get; set; }
    }
}