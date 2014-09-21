/*
 *  Imports the commands and adds them to the Action List to be accessed in the actions window. 
 * 
 */

using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Windows;

namespace Andromeda.Command
{
    public class Commands
    {
        private ObservableCollection<Action> actionsList = new ObservableCollection<Action>();
        public ObservableCollection<Action> ActionsList { get { return actionsList; } }

        public Commands() { ImportCommandsList(); }

        public void ImportCommandsList()
        {
            actionsList.Add(new GetLoggedOnUser());
            actionsList.Add(new PingTest());
            actionsList.Add(new RunRemoteCommand());
            actionsList.Add(new DebugToConsole());
        }

        /* -- External commands are still not supported yet. This will be uncommented when that support is available.
         * 
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
         */

        
    }
}
