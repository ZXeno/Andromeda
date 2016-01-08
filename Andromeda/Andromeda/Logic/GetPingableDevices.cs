using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Andromeda.Infrastructure;
using Andromeda.Model;

namespace Andromeda
{
    public class GetPingableDevices
    {
        private static Configuration Config { get { return ConfigManager.CurrentConfig; } }

        public static List<string> GetDevices(List<string> devlist)
        {
            var destinationdirectory = Config.ResultsDirectory;
            var netConn = new NetworkConnections();

            List<string> successList = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var device in devlist)
            {
                bool didResolve;
                IPHostEntry hostentry;
                try
                {
                    hostentry = Dns.GetHostEntry(device);
                    didResolve = true;
                }
                catch (Exception)
                {
                    var returnMsg = string.Format(device + " Connection Error: Could not resolve host. Added to failed list file.");
                    ResultConsole.Instance.AddConsoleLine(returnMsg);
                    sb.AppendLine(returnMsg);
                    didResolve = false;
                }

                if (didResolve)
                {
                    try
                    {
                        var hostname = netConn.PingTest(device);

                        // If the ping reply isn't null...
                        if (hostname != null)
                        {
                            // based on our connection status, return a message
                            switch (hostname.Status)
                            {
                                case IPStatus.Success:
                                    successList.Add(device);
                                    break;

                                case IPStatus.TimedOut:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Timeout. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.BadDestination:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Bad Destination. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.BadHeader:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Bad Header. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.BadOption:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Bad Option. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.BadRoute:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Bad Route. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.DestinationHostUnreachable:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Destination Host Unreachable. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.DestinationNetworkUnreachable:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Destination Network Unreachable. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.DestinationPortUnreachable:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Destination Port Unreachable. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.DestinationProtocolUnreachable:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Destination Network Unreachable. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.DestinationScopeMismatch:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Destination Scope Mismatch. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.HardwareError:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Hardware Error. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.IcmpError:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: ICMP Error. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.NoResources:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: No Resources. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.TimeExceeded:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Time Exceeded. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.TtlExpired:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: TTL Expired. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.PacketTooBig:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Packet Too Big. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.SourceQuench:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Source Quench. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.TtlReassemblyTimeExceeded:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: TTL Reassembly Time Exceeded. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.ParameterProblem:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: IPSTATUS.PARAMETERPROBLEM . Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                case IPStatus.UnrecognizedNextHeader:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Unrecognized Next Header. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;

                                default:
                                    sb.AppendLine(device);
                                    ResultConsole.Instance.AddConsoleLine(device + " failed to connect: Unknown reason. Added to failed list file.");
                                    ProgressData.OnUpdateProgressBar(1);
                                    break;
                            }
                        }
                        else
                        {
                            // if it isn't null, but fails anyway, I'm not exactly certain why we would have an error.
                            sb.AppendLine(device);
                            ResultConsole.Instance.AddConsoleLine(device + " failed to connect. Added to failed list file.");
                            ProgressData.OnUpdateProgressBar(1);
                        }
                    }
                    catch (PingException ex)
                    {
                        var returnMsg = string.Format(device + " threw exception - Connection Error: {0}", ex.Message + ": " + ex.InnerException + " Added to failed list file.");
                        sb.AppendLine(returnMsg);
                        ResultConsole.Instance.AddConsoleLine(returnMsg);
                        ProgressData.OnUpdateProgressBar(1);
                    }
                    catch (SocketException ex)
                    {
                        var returnMsg = string.Format(device + " threw exception - Connection Error: {0}", ex.Message + ": " + ex.InnerException + " Added to failed list file.");
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

                    try
                    {
                        outfile.WriteAsync(sce.ToString());
                        outfile.Close();
                    }
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