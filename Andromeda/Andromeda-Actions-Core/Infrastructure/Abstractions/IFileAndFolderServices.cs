namespace Andromeda_Actions_Core.Infrastructure
{
    public interface IFileAndFolderServices
    {
        void CreateNewTextFile(string filepath);
        void CreateRemoteTextFile(string filepath, string contents);
        void WriteToTextFile(string filepath, string contents);
        void CleanDirectory(string device, string path);
        bool ValidateDirectoryExists(string device, string path, string actionName);
        bool ValidateFileExists(string device, string path, string actionName);
    }
}