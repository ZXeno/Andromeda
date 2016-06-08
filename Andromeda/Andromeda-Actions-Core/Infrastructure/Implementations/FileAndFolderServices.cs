using System;
using System.IO;
using System.Text;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class FileAndFolderServices : IFileAndFolderServices
    {
        public async void CreateNewTextFile(string filepath)
        {
            if (File.Exists(filepath)) { return; }

            using (var outfile = new StreamWriter(filepath, true))
            {
                try { await outfile.WriteAsync(""); }
                catch (Exception e)
                {
                    throw new Exception($"Unable to create file. Error: {e.Message}");
                }
            }
        }

        public async void CreateRemoteTextFile(string filepath, string contents)
        {
            if (File.Exists(filepath)) return;

            using (var outfile = new StreamWriter(filepath, true))
            {
                try
                {
                    await outfile.WriteAsync(contents);
                    outfile.Close();
                }
                catch (Exception e)
                {
                    Logger.Log($"Unable to create remote file {filepath} Exception: {e.Message}");
                    ResultConsole.Instance.AddConsoleLine($"Unable to create remote file {filepath} Exception: {e.Message}");
                }
            }
        }

        public void WriteToTextFile(string filepath, string contents)
        {
            var sb = new StringBuilder();
            sb.Append(contents);

            using (var outfile = new StreamWriter(filepath, true))
            {
                outfile.WriteAsync(sb.ToString());
            }
        }

        public void CleanDirectory(string device, string path)
        {
            var fullPath = $"\\\\{device}\\C$" + path;

            try
            {
                Directory.Delete(fullPath, true);
                Logger.Log($"Cleaned directory {fullPath}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"Failed to clean directory {fullPath}. Due to exception {ex.Message}");
                Logger.Log($"Failed to clean directory {fullPath}. Due to exception {ex.Message} Inner exception: {ex.InnerException}");
            }
        }

        public bool ValidateDirectoryExists(string device, string path, string actionName)
        {
            try
            {
                return Directory.Exists($"\\\\{device}\\C$\\{path}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"There was an exception when validating the directory {path} for machine: {device}");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                Logger.Log($"{actionName} failed to validate directory: \\\\{device}\\C$\\{path}");
                return false;
            }
        }

        public bool ValidateFileExists(string device, string path, string actionName)
        {
            try
            {
                return File.Exists($"\\\\{device}\\C${path}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"There was an exception when validating the file {path} for machine: {device}");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                Logger.Log($"{actionName} failed to validate file: \\\\{device}\\C$\\{path}");
                return false;
            }
        }
    }
}