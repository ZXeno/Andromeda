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

namespace Andromeda.Command
{
    public class Commands
    {
        private List<Action> actionsList = new List<Action>();
        public List<Action> ActionsList { get { return actionsList; } }

        public Commands() { ImportCommandsList(); }

        public void ImportCommandsList()
        {
            actionsList.Add(new PingTest("Ping Test", "Runs a ping test against the device list.", ActionGroup.Other));
        }

        public static void ImportExternalCommands(string commandsPath)
        {
            if (XMLImport.FileExists(commandsPath))
            {
                try { XMLImport.GetXMLFileData(commandsPath); }
                catch (Exception e)
                {
                    MessageBox.Show("There was an error: \n" + e.Message);
                }
            }
            else
            {
                MessageBox.Show("I'm sorry, Dave. I'm affraid I can't find the commands file. \n There is an explicit need for this file. \n Please replace the commands file with the most current one.");
            }
        }
    }
}
