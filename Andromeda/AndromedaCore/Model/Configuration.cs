namespace AndromedaCore.Model
{
    public class Configuration
    {
        public string SaveFileVersion { get; set; }
        public string DataFilePath { get; set; }
        public bool EnableDeviceCountWarning { get; set; }
        public int DeviceCountWarningThreshold { get; set; }
        public bool SaveOfflineComputers { get; set; }
        public bool SaveOnlineComputers { get; set; }
        public string ResultsDirectory { get; set; }
        public string ComponentDirectory { get; set; }
        public string FailedConnectListFile { get; set; }
        public string SuccessfulConnectionListFile { get; set; }
    }
}