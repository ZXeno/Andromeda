using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Andromeda_Actions_Core.Model;

namespace Andromeda_Actions_Core.Infrastructure
{
    public class GetPingableDevices
    {
        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }

        public static List<string> GetDevices(List<string> devlist)
        {
            var destinationdirectory = Config.ResultsDirectory;

            List<string> successList = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var device in devlist)
            {
                bool didResolve = NetworkConnections.DnsResolvesSuccessfully(device);

                if (didResolve)
                {
                    try
                    {
                        var statusmsg = NetworkConnections.Pingable(device);
                        sb.AppendLine(device + " " + NetworkConnections.GetIpStatusMessage(statusmsg));
                        if (statusmsg == IPStatus.Success)
                        {
                            successList.Add(device);
                        }
                    }
                    catch (PingException ex)
                    {
                        var returnMsg = string.Format(device + " threw exception - Connection Error: {0}", ex.Message + ": " + ex.InnerException + " Added to failed list file.");
                        sb.AppendLine(returnMsg);
                        ResultConsole.Instance.AddConsoleLine(returnMsg);

                    }
                    catch (SocketException ex)
                    {
                        var returnMsg = string.Format(device + " threw exception - Connection Error: {0}", ex.Message + ": " + ex.InnerException + " Added to failed list file.");
                        sb.AppendLine(returnMsg);
                        ResultConsole.Instance.AddConsoleLine(returnMsg);
                    }
                }
            }

            if (Config.SaveOfflineComputers)
            {
                if (File.Exists(destinationdirectory + "\\" + Config.FailedConnectListFile))
                {
                    File.Delete(destinationdirectory + "\\" + Config.FailedConnectListFile);
                }

                using (StreamWriter outfile = new StreamWriter(destinationdirectory + "\\" + Config.FailedConnectListFile, true))
                {
                    try
                    {
                        outfile.WriteAsync(sb.ToString());
                    }
                    catch (Exception e)
                    {
                        var msg = "Unable to write to " + Config.FailedConnectListFile + ". \n" + e.Message;
                        ResultConsole.Instance.AddConsoleLine(msg);
                        Logger.Log(msg);
                    }
                }
            }

            if (Config.SaveOnlineComputers)
            {
                if (File.Exists(destinationdirectory + "\\" + Config.SuccessfulConnectionListFile))
                {
                    File.Delete(destinationdirectory + "\\" + Config.SuccessfulConnectionListFile);
                }

                using (StreamWriter outfile = new StreamWriter(destinationdirectory + "\\" + Config.SuccessfulConnectionListFile, true))
                {
                    StringBuilder sce = new StringBuilder();
                    foreach (var sc in successList)
                    {
                        sce.AppendLine(sc);
                    }

                    try
                    {
                        outfile.WriteAsync(sce.ToString());
                        outfile.Close();
                    }
                    catch (Exception e)
                    {
                        var msg = "Unable to write to " + Config.SuccessfulConnectionListFile + ". \n" + e.Message;
                        ResultConsole.Instance.AddConsoleLine(msg);
                        Logger.Log(msg);
                    }
                }
            }

            return successList;
        }
    }
}