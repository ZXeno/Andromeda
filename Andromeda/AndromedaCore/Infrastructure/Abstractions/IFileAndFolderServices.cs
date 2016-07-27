namespace AndromedaCore.Infrastructure
{
    public interface IFileAndFolderServices
    {
        void CreateNewTextFile(string filepath);
        void CreateRemoteTextFile(string filepath, string contents, ILoggerService logger);
        void WriteToTextFile(string filepath, string contents, ILoggerService logger);
        void CleanDirectory(string device, string path, ILoggerService logger);
        bool ValidateDirectoryExists(string device, string path, string actionName, ILoggerService logger);
        bool ValidateFileExists(string device, string path, string actionName, ILoggerService logger);
    }
}