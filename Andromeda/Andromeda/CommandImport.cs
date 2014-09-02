/*
 *  Imports the Command.XML file from .\Tasks directory, then parses them and prepares them for display in the program window. 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

namespace Andromeda
{
    public class CommandImport
    {
        public static void ImportCommands(string commandsPath)
        {
            if (XMLImport.FileExists(commandsPath))
            {
                try { XMLImport.GetXMLFileData(commandsPath); }
                catch (Exception e)
                {
                    MessageBox.Show("There was an error: \n" + e.ToString());
                }
            }
            else
            {
                MessageBox.Show("I'm sorry, Dave. I'm affraid I can't find the commands file. \n There is an explicit need for this file. \n Please replace the commands file with the most current one.");

            }
        }
    }
}
