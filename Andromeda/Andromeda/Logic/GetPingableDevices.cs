using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Andromeda.Model;

namespace Andromeda
{
    public class GetPingableDevices
    {
        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }

        public static List<string> GetDevices(List<string> devlist)
        {
            string destinationdirectory = Config.ResultsDirectory;
            var netConn = new NetworkConnections();

            List<string> successList = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var d in devlist)
            {
                bool didResolve;
                IPHostEntry hostentry;
                try
                {
                    hostentry = Dns.GetHostEntry(d);
                    didResolve = true;
                }
                catch (Exception)
                {
                    var returnMsg = string.Format(d + " Connection Error: Could not resolve host.");
                    ResultConsole.Instance.AddConsoleLine(returnMsg);
                    sb.AppendLine(returnMsg);
                    didResolve = false;
                }

                if (didResolve)
                {
                    try
                    {
                        var hostname = netConn.PingTest(d);

                        // If the ping reply isn't null...
                        if (hostname != null)
                        {
                            // based on our connection status, return a message
                            switch (hostname.Status)
                            {
                                case IPStatus.Success:
                                    successList.Add(d);
                                    break;
                                case IPStatus.TimedOut:
                                    sb.AppendLine(d);
                                    ResultConsole.Instance.AddConsoleLine(d + " failed to connect: Timeout. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;
                                default:
                                    sb.AppendLine(d);
                                    ResultConsole.Instance.AddConsoleLine(d + " failed to connect. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;
                            }
                        }
                        else
                        {
                            // if it isn't null, but fails anyway, I'm not exactly certain why we would have an error.
                            sb.AppendLine(d);
                            ResultConsole.Instance.AddConsoleLine(d + " failed to connect. Added to failed list file.");
                            ProgressData.OnUpdateProgressBar(1);
                        }
                    }
                    catch (PingException ex)
                    {
                        var returnMsg = string.Format(d + " Connection Error: {0}", ex.Message + ": " + ex.InnerException);
                        sb.AppendLine(returnMsg);
                        ResultConsole.Instance.AddConsoleLine(returnMsg);
                        ProgressData.OnUpdateProgressBar(1);
                    }
                    catch (SocketException ex)
                    {
                        var returnMsg = string.Format(d + " Connection Error: {0}", ex.Message + ": " + ex.InnerException);
                        sb.AppendLine(returnMsg);
                        ResultConsole.Instance.AddConsoleLine(returnMsg);
                        ProgressData.OnUpdateProgressBar(1);
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
                        MessageBox.Show("Unable to write to " + Config.FailedConnectListFile + ". \n" + e.InnerException);
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

                    try { outfile.WriteAsync(sce.ToString()); }
                    catch (Exception e)
                    {
                        MessageBox.Show("Unable to write to " + Config.SuccessfulConnectionListFile + ". \n" + e.InnerException);
                    }
                }
            }

            return successList;
        } 
    }
}