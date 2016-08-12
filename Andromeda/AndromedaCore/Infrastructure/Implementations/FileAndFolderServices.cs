using System;
using System.IO;
using System.Text;
using AndromedaCore.Managers;

namespace AndromedaCore.Infrastructure
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

        public async void CreateRemoteTextFile(string filepath, string contents, ILoggerService logger)
        {
            if (File.Exists(filepath)) return;
            
            try
            {
                using (var outfile = new StreamWriter(filepath, true))
                {
                    await outfile.WriteAsync(contents);
                    outfile.Close();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Unable to create remote file {filepath}", e);
            }
        }

        public void WriteToTextFile(string filepath, string contents, ILoggerService logger)
        {
            var sb = new StringBuilder();
            sb.Append(contents);

            using (var outfile = new StreamWriter(filepath, true))
            {
                try
                {
                    outfile.WriteAsync(sb.ToString());
                }
                catch (Exception e)
                {
                    logger.LogError($"Unable to write to directory {filepath}", e);
                }
                
            }
        }

        public void CleanDirectory(string device, string path, ILoggerService logger)
        {
            var fullPath = $"\\\\{device}\\C$" + path;

            try
            {
                Directory.Delete(fullPath, true);
                logger.LogMessage($"Cleaned directory {fullPath}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"Failed to clean directory {fullPath}. Due to exception {ex.Message}");
                logger.LogError($"Failed to clean directory {fullPath}.", ex);
            }
        }

        public bool ValidateDirectoryExists(string device, string path, string actionName, ILoggerService logger)
        {
            try
            {
                return Directory.Exists($"\\\\{device}\\C$\\{path}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"There was an exception when validating the directory {path} for machine: {device}");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                logger.LogWarning($"{actionName} failed to validate directory: \\\\{device}\\C$\\{path}", null);
                return false;
            }
        }

        public bool ValidateFileExists(string device, string path, string actionName, ILoggerService logger)
        {
            try
            {
                return File.Exists($"\\\\{device}\\C${path}");
            }
            catch (Exception ex)
            {
                ResultConsole.Instance.AddConsoleLine($"There was an exception when validating the file {path} for machine: {device}");
                ResultConsole.Instance.AddConsoleLine(ex.Message);
                logger.LogWarning($"{actionName} failed to validate file: \\\\{device}\\C$\\{path}", ex);
                return false;
            }
        }
    }
}