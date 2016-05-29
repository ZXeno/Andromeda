using System;
using System.Text;
using System.IO;

namespace Andromeda_Actions_Core.Infrastructure
{
    class WriteToTextFile
    {
        // write to one-time log file. Does not check for previous file of same name and will overwrite it!
        public static void WriteToLogFile(string filepath, string contents)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(contents);

            using (StreamWriter outfile = new StreamWriter(filepath, true))
            {
                try { outfile.WriteAsync(sb.ToString()); }
                catch (Exception e)
                {
                    Logger.Log("Unable to write to log file. \n" + e.HResult);
                    ResultConsole.Instance.AddConsoleLine("Unable to write to log file.");
                }
            }
        }

        // Add line to existing file
        public static void AddLineToFile(string filepath, string line)
        {
            StringBuilder sb = new StringBuilder();

            if (File.Exists(filepath))
            {
                sb.AppendLine(line);
            }
            else
            {
                ResultConsole.Instance.AddConsoleLine("Strangely, there is no file at: (" + filepath + ") \n A new file will be created.");
                CreateNewLogFile(filepath);
                sb.Append(ResultConsole.Instance.ConsoleString);
            }

            using (StreamWriter outfile = new StreamWriter(filepath, true))
            {
                try { outfile.WriteAsync(sb.ToString()); }
                catch (Exception e)
                {
                    throw new Exception("Unable to write to file. \n" + e.Message);
                }
            }
        }

        // Create new empty log file
        public async static void CreateNewLogFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                //ValidateDestinationExists();

                using (StreamWriter outfile = new StreamWriter(filepath, true))
                {
                    try { await outfile.WriteAsync(""); }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to create log file. \n Returned error: " + e.Message);
                    }
                }
            }
        }

        // Create Remote Text File
        public async static void CreateRemoteTextFile(string filepath, string contents)
        {
            if (!File.Exists(filepath))
            {
                using (StreamWriter outfile = new StreamWriter(filepath, true))
                {
                    try
                    {
                        await outfile.WriteAsync(contents);
                        outfile.Close();
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Unable to create remote file " + filepath + " Exception: " + e.Message);
                        ResultConsole.Instance.AddConsoleLine("Unable to create remote file " + filepath + " Exception: " + e.Message);
                    }
                }
            }
        }
    }
}
