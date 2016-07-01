using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace AndromedaSetup
{
    internal class Installer
    {
        private const string DisplayName = "Andromeda";
        private const string Publisher = "Jonathan Cain";
        private const string UrlInfoAbout = "";
        private const string Contact = "xivdot@gmail.com";
        private const string UninstallGuid = "{E9B97EA3-B29B-4154-AB96-E6E40C89C12F}";
        private const string UninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private string UninstallerLocation => Program.InstallLogPath;
        private const string UninstallerName = "AndromedaSetup.exe";
        
        private string _installDirectory = "";

        public void Install(string providedInstallPath)
        {
            _installDirectory = providedInstallPath;

            List<string> installLogContent = new List<string>();

            try
            {
                var config = new InstallConfig();
                if (!config.LoadConfigFile()) { return; }

                foreach (var process in Process.GetProcessesByName("Andromeda"))
                {
                    WriteConsole("Ending process " + process.ProcessName);
                    process.Kill();
                }

                Thread.Sleep(1000);

                if (Directory.Exists(_installDirectory))
                {
                    var files = Directory.EnumerateFiles(_installDirectory).ToList();
                    var directories = Directory.EnumerateDirectories(_installDirectory).ToList();

                    directories.ForEach(dir =>
                    {
                        files.AddRange(Directory.EnumerateFiles(dir).ToList());
                    });
                    
                        files.ForEach(f =>
                        {
                            try
                            {
                                if (File.Exists(f))
                                {
                                    File.Delete(f);
                                }
                            }
                            catch (Exception e)
                            {
                                WriteConsole($"{e.Message}, it will be ignored.");
                            }
                        });
                    
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(_installDirectory);
                    }
                    catch (Exception e)
                    {
                        WriteConsole($"{e.Message}");
                        WriteConsole("Installation has been aborted.");
                        return;
                    }
                }

                installLogContent = new List<string>(config.InstallableItems.Count);
                config.InstallableItems.ForEach(item =>
                {
                    if (!File.Exists(item.Source))
                    {
                        throw new Exception("Could not find source item " + item.Item);
                    }

                    string destPath = "";
                    if (item.Destination.Contains("{path}"))
                    {
                        destPath = item.Destination.Replace("{path}", _installDirectory);
                    }
                    else if (item.Destination.Contains("{user}"))
                    {
                        destPath = item.Destination.Replace("{user}", Environment.UserName);
                    }

                    if (string.IsNullOrWhiteSpace(destPath))
                    {
                        WriteConsole($"Unable to install file {item.Item}. Could not resolve destination from installer file.");
                        return;
                    }

                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }

                    var fileName = item.Source.Split(new char[] { '\\' }).Last();

                    File.Copy(item.Source, destPath + "\\" + fileName, true);

                    installLogContent.Add(destPath + "\\" + fileName);

                    WriteConsole($"Copied {fileName} to {destPath}");
                });

                var uninstaller = Assembly.GetExecutingAssembly().Location;

                try
                {
                    Directory.CreateDirectory(UninstallerLocation);
                    File.Copy(uninstaller, $"{UninstallerLocation}\\{UninstallerName}", true);
                }
                catch (Exception e)
                {
                    WriteConsole($"Unable to copy uninstaller to {UninstallerLocation}, see the log at {Program.InstallLogPath}\\{Program.InstallLogFileName} for instructions on how to correct this.");
                    installLogContent.Add($"[ERROR]There was an error copying the uninstaller. Copy the installer executable \"{UninstallerName}\" to {UninstallerLocation}. \n[ERROR] {e.Message}");
                }

                CreateUninstallInfo();

            }
            catch (Exception ex)
            {
                WriteConsole(ex.Message);
            }

            try
            {
                WriteInstallLog(installLogContent);
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
            }
            
        }

        public void WriteInstallLog(List<string> paths)
        {
            var sb = new StringBuilder();

            foreach (var path in paths)
            {
                sb.AppendLine(path);
            }

            var lp = Program.InstallLogPath + "\\" + Program.InstallLogFileName;

            using (var outfile = new StreamWriter(lp, true))
            {
                try
                {
                    outfile.WriteAsync(sb.ToString());
                }
                catch (Exception e)
                {
                    WriteConsole($"Unable create install log {lp}. Andromeda is fully installed, but will need to be uninstalled manually.");
                    WriteConsole($"{e.Message}");
                }
            }
        }

        private void WriteUninstallLog(string content)
        {
            var lp = "C:\\Windows\\Temp\\AndromedaUninstallLog.log";

            using (var outfile = new StreamWriter(lp, true))
            {
                try
                {
                    outfile.WriteAsync(content);
                }
                catch (Exception e)
                {
                    WriteConsole("Unable create uninstall log.");
                    WriteConsole($"{e.Message}");
                }
            }
        }

        private void CreateUninstallInfo()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(UninstallRegKeyPath, true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        key = parent.OpenSubKey(UninstallGuid, true) ?? parent.CreateSubKey(UninstallGuid);

                        if (key == null)
                        {
                            throw new Exception($"Unable to create uninstaller '{UninstallRegKeyPath}\\{UninstallGuid}'");
                        }

                        Assembly asm = GetType().Assembly;
                        Version v = asm.GetName().Version;
                        string exe = $"{UninstallerLocation}\\{UninstallerName}";

                        key.SetValue("DisplayName", DisplayName);
                        key.SetValue("ApplicationVersion", v.ToString());
                        key.SetValue("Publisher", Publisher);
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString(3));
                        key.SetValue("URLInfoAbout", UrlInfoAbout);
                        key.SetValue("Contact", Contact);
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", exe + " /uninstall");
                        key.SetValue("InstallationPath", _installDirectory);
                    }
                    finally
                    {
                        key?.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred writing uninstall information to the registry. Andromeda is fully installed but can only be uninstalled manually.", ex);
                }
            }
        }

        public void Uninstall()
        {
            var uninstallLogContent = new StringBuilder();

            try
            {
                string installPath;
                using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(UninstallRegKeyPath, true))
                {
                    RegistryKey key = null;

                    try
                    {
                        key = parent.OpenSubKey(UninstallGuid, true) ?? parent.CreateSubKey(UninstallGuid);

                        if (key == null)
                        {
                            throw new Exception($"Unable to open uninstaller registry key '{UninstallRegKeyPath}\\{UninstallGuid}'");
                        }

                        installPath = key.GetValue("InstallationPath").ToString();
                    }
                    catch (Exception e)
                    {
                        WriteConsole(e.Message);
                        Console.ReadLine();
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(installPath))
                {
                    throw new Exception($"Unable to find or read installation path from registry.");
                }

                var file = new StreamReader($"{UninstallerLocation}\\{UninstallerName}");
                
                var filePaths = new List<string>();
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("[ERROR]")) { continue; }

                    filePaths.Add(line);
                }
                file.Close();

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            WriteConsole($"{filePath} deleted successfully.");
                            uninstallLogContent.AppendLine($"{filePath} deleted successfully.");
                        }
                    }
                    catch (Exception e)
                    {
                        uninstallLogContent.AppendLine($"{e.Message}. It will need to be removed manually.");
                    }
                }

                WriteConsole("Cleaning up reamining files...");

                var remainingFiles = Directory.EnumerateFiles(installPath).ToList();

                if (remainingFiles.Count > 0)
                {
                    foreach (var rf in remainingFiles)
                    {
                        try
                        {
                            File.Delete(rf);
                        }
                        catch (Exception)
                        {
                            uninstallLogContent.AppendLine($"[ERROR] Unable to remove file {rf}");
                        }
                    }
                }

                try
                {
                    Directory.Delete(installPath, true);
                }
                catch (Exception)
                {
                    uninstallLogContent.AppendLine($"[ERROR] Unable to remove directory {installPath}");
                }
            }
            catch (Exception e)
            {
                WriteConsole(e.Message);
            }
            
            WriteUninstallLog(uninstallLogContent.ToString());
            RemoveUninstaller();
        }



        private void RemoveUninstaller()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(UninstallRegKeyPath, true))
            {
                if (key == null)
                {
                    return;
                }
                try
                {
                    RegistryKey child = key.OpenSubKey(UninstallGuid);
                    if (child != null)
                    {
                        child.Close();
                        key.DeleteSubKey(UninstallGuid);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "An error occurred removing uninstall information from the registry. Andromeda will still show up in the Programs and Features list." + 
                        $" To remove it manually delete the entry HKLM\\{UninstallRegKeyPath}\\{UninstallGuid}", ex);
                }
            }
        }

        private void WriteConsole(string msg)
        {
            if (!Program.IsSilentInstall)
            {
                Console.WriteLine(msg);
            }
        }
    }
}