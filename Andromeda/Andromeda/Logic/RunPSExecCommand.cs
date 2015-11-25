using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Andromeda.Model;

namespace Andromeda
{
    public class RunPSExecCommand
    {
        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }
        public static void RunOnDeviceWithAuthentication(string device, string commandline, CredToken creds)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo(Config.ComponentDirectory + "\\PsExec.exe");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.WindowStyle = ProcessWindowStyle.Minimized;
                psi.CreateNoWindow = true;
                // For whatever reason, making everything a string literal fixed a problem with making this work correctly
                psi.Arguments = @"\\" + device + @" -u " + creds.Domain + @"\" + creds.User + @" -p " + creds.GetInsecurePasswordString() + @" " + commandline;
                var loggableArguments = @"\\" + device + @" -u " + creds.Domain + @"\" + creds.User + @" -p [REDACTED] " + commandline;
                process.StartInfo = psi;

                Logger.Log("Beginning PsExec attempt on " + device + " with following command line options: " + loggableArguments);

                process.Start();
                process.WaitForExit(30000);

                var stdResult = RemoveEmptyLines(process.StandardOutput.ReadToEnd());
                var errResult = RemoveEmptyLines(process.StandardError.ReadToEnd());

                ResultConsole.Instance.AddConsoleLine(errResult);
                ResultConsole.Instance.AddConsoleLine(stdResult);
                Logger.Log(errResult);
            }
            catch (Exception ex)
            {
                var exceptionResult = string.Format("Exception running command on device " + device + ".\n", ex.Message, ex.InnerException);
                ResultConsole.Instance.AddConsoleLine(exceptionResult);
            }
        }

        public static void RunOnDeviceWithoutAuthentication(string device, string commandline)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo(Config.ComponentDirectory + "\\PsExec.exe");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.WindowStyle = ProcessWindowStyle.Minimized;
                psi.CreateNoWindow = true;
                // For whatever reason, making everything a string literal fixed a problem with making this work correctly
                psi.Arguments = @"\\" + device + @" " + commandline;
                var loggableArguments = @"\\" + device + @" -u peacehealth\!joncai -i " + commandline;
                process.StartInfo = psi;

                Logger.Log("Beginning PsExec (NO AUTH) attempt on " + device + " with following command line options: " + loggableArguments);

                process.Start();
                process.WaitForExit(30000);

                var stdResult = RemoveEmptyLines(process.StandardOutput.ReadToEnd());
                var errResult = RemoveEmptyLines(process.StandardError.ReadToEnd());

                ResultConsole.Instance.AddConsoleLine(errResult);
                ResultConsole.Instance.AddConsoleLine(stdResult);
                Logger.Log(errResult);
            }
            catch (Exception ex)
            {
                var exceptionResult = string.Format("Exception running command on device " + device + ".\n", ex.Message, ex.InnerException);
                ResultConsole.Instance.AddConsoleLine(exceptionResult);
            }
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
    }
}