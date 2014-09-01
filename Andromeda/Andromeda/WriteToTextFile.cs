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
        public async static void WriteToLogFile(string filepath, string contents)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(contents);
            sb.AppendLine();

            using (StreamWriter outfile = new StreamWriter(filepath, true))
            {
                await outfile.WriteAsync(sb.ToString());
            }
        }
    }
}
