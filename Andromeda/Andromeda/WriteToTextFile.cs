using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace Andromeda
{
    class WriteToTextFile
    {
        // write to one-time log file. Does not check for previous file of same name and will overwrite it!
        public async static void WriteToLogFile(string filepath, string contents)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(contents);
            sb.AppendLine();

            using (StreamWriter outfile = new StreamWriter(filepath, true))
            {
                try { await outfile.WriteAsync(sb.ToString()); }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to write to log file. \n" + e.HResult.ToString() + "\n \n Andromeda is now closing...");
                    Application.Current.Shutdown();
                }
            }
        }

        // Add line to existing log file
        public async static void AddLineToFile(string filepath, string line)
        {
            StringBuilder sb = new StringBuilder();

            if (File.Exists(filepath))
            {
                string file = File.ReadAllText(filepath);

                sb.Append(file);
                sb.AppendLine(line);
            }
            else
            {
                ResultConsole.AddConsoleLine("Strangely, there is no log file at: (" + filepath + ") \n A new logfile will be created.");
                CreateNewLogFile(filepath);
                sb.Append(ResultConsole.ConsoleString);
            }

            using (StreamWriter outfile = new StreamWriter(filepath, true))
            {
                try { await outfile.WriteAsync(sb.ToString()); }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to write to log file. \n" + e.HResult.ToString() + "\n \n Andromeda is now closing...");
                    Application.Current.Shutdown();
                }
            }
        }

        // Create new empty log file
        public async static void CreateNewLogFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                using (StreamWriter outfile = new StreamWriter(filepath, true))
                {
                    try { await outfile.WriteAsync(""); }
                    catch (Exception e)
                    {
                        MessageBox.Show("Unable to create log file. \n Returned error: " + e.HResult.ToString() + "\n \n Andromeda is now closing...");
                        Application.Current.Shutdown();
                    }
                }
            }
            else { return; }
        }
    }
}
