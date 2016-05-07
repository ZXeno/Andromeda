using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Andromeda_Actions_Core.Infrastructure;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core
{
    public class RunPsExecCommand
    {
        private static Configuration Config => ConfigManager.CurrentConfig;

        public static void RunOnDeviceWithAuthentication(string device, string commandline, CredToken creds)
        {
            // For whatever reason, making everything a string literal fixed a problem with making this work correctly
            var loggableArguments = @"\\" + device + @" -u " + creds.Domain + @"\" + creds.User + @" -p [REDACTED] " + commandline;
            Logger.Log("Beginning PsExec attempt on " + device + " with following command line options: " + loggableArguments);

            RunPsExec(device, commandline, creds);
        }

        public static void RunOnDeviceWithoutAuthentication(string device, string commandline)
        {
            // For whatever reason, making everything a string literal fixed a problem with making this work correctly
            var loggableArguments = @"\\" + device + @" " + commandline;
            Logger.Log("Beginning PsExec (NO AUTH) attempt on " + device + " with following command line options: " + loggableArguments);

            RunPsExec(device, commandline);
        }

        private static void RunPsExec(string device, string arguments, CredToken creds = null)
        {
            ForceCleanRemotePsExeSvc(device);

            var process = GeneratePsExecProcess();

            try
            {
                if (creds != null)
                {
                    process.StartInfo.Arguments = @"\\" + device + @" -u " + creds.Domain + @"\" + creds.User + @" -p " + SecureStringHelper.GetInsecureString(creds.SecurePassword) + @" " + arguments;
                }
                else
                {
                    process.StartInfo.Arguments = @"\\" + device + @" " + arguments;
                }

                process.Start();
                Logger.Log("PSExec process started with start ID: " + process.Id);
                process.WaitForExit(60000);

                if (!process.HasExited)
                {
                    process.Kill();
                    Logger.Log("Killed process " + process.Id);
                }

                var stdOutput = process.StandardOutput.ReadToEnd();
                var errOutput = process.StandardError.ReadToEnd();

                var stdResult = RemoveEmptyLines(stdOutput);
                var errResult = RemoveEmptyLines(errOutput);

                if (string.IsNullOrWhiteSpace(stdOutput) && string.IsNullOrWhiteSpace(errResult))
                {
                    throw new Exception("Device did not return a value. Please run the command on the device manually. \n PsExec command line: " + process.StartInfo.Arguments);
                }

                ResultConsole.Instance.AddConsoleLine(errResult);
                ResultConsole.Instance.AddConsoleLine(stdResult);
                Logger.Log(errResult);
            }
            catch (Exception ex)
            {
                var exceptionResult = "Exception running command on device " + device + ". " + ex.Message;
                ResultConsole.Instance.AddConsoleLine(exceptionResult);
            }

            Thread.Sleep(1000);
        }

        private static Process GeneratePsExecProcess()
        {
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(Config.ComponentDirectory + "\\PsExec.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;

            process.StartInfo = psi;

            return process;
        }

        private static string RemoveEmptyLines(string str)
        {
            List<string> lineList = new List<string>(str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
            lineList = lineList.Where(x => x != " ").ToList();
            string result = "";

            foreach (var line in lineList)
            {
                result += line + "\n";
            }

            return result;
        }

        private static void ForceCleanRemotePsExeSvc(string device)
        {
            var filepath = "C:\\Windows\\scrubremotepsexesvc.bat";
            var sb = new StringBuilder();

            sb.AppendLine("sc \\" + device + " stop PsExeSvc");
            sb.AppendLine("sc \\" + device + " delete PsExeSvc");
            sb.AppendLine("del /F /A: " + device + "\\admin$\\PsExeSvc.exe");
            sb.AppendLine("del /F /A: " + device + "\\admin$\\System32\\PsExeSvc.exe");

            if (!File.Exists(filepath))
            {
                using (StreamWriter outfile = new StreamWriter(filepath, false))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                        outfile.Close();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Unable to create Remote PsExec scrubber batch file " + filepath + " Exception: " + e.Message);
                        ResultConsole.Instance.AddConsoleLine("Unable to create Remote PsExec scrubber batch file " + filepath + " Exception: " + e.Message);
                    }
                }
            }

            Thread.Sleep(500);

            var purgeproc = new Process();
            var psi = new ProcessStartInfo("cmd.exe");
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.Arguments = "/C " + filepath;
            purgeproc.StartInfo = psi;

            purgeproc.Start();
            purgeproc.WaitForExit(3000);
            Thread.Sleep(1000);

            if (!purgeproc.HasExited)
            {
                purgeproc.Kill();
                Logger.Log("Killed process " + purgeproc.Id);

                foreach (var process in Process.GetProcessesByName("sc.exe"))
                {
                    Logger.Log("Killed process sc.exe with id " + process.Id);
                    process.Kill();
                }

            }

            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception)
                {
                    Logger.Log("Unable to remove the scrubremotepsexesvc file. It will need to be manually cleaned.");
                }
            }

            Thread.Sleep(500);
        }
    }
}