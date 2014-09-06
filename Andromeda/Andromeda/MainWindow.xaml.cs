﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using Andromeda.Command;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string WorkingPath = Environment.CurrentDirectory;
        public const string ConfigFileName = "config.dat";
        public const string CommandsFileName = "commands.xml";
        public const string CommandsDir = "\\commands\\";
        public const string ResultsDir = "\\results\\";
        public XmlDocument ConfigFile;
        //public XmlDocument CommandsFile;

        public Config configuration;
        public Commands Commander { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeConsole();
            ImportCommands();
            ImportConfiguration();
            CREDS_LABEL.Content = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public void ImportConfiguration()
        {
            string p = WorkingPath + "\\" + ConfigFileName;
            if (CheckForConfigFile())
            {
                try { ConfigFile = XMLImport.GetXMLFileData(p); }
                catch (FileNotFoundException fnf)
                {
                    MessageBox.Show("File not found. \n Exception: " + fnf.HResult.ToString());
                    App.Current.Shutdown();
                }

                ResultConsole.AddConsoleLine("Configuration file found.");

                if (ConfigFile != null)
                {
                    ResultConsole.AddConsoleLine("Config file loaded. - " + p);
                }
            }
            else
            {
                ResultConsole.AddConsoleLine("No config file found!");
                ResultConsole.AddConsoleLine("Generating new config file...");
                File.CreateText(p);
                CreateConfigFile(p);
            }
        }

        public void ImportCommands()
        {
            Commander = new Commands();
            AVAIL_ACTS_LISTBOX.ItemsSource = Commander.ActionsList;
            AVAIL_ACTS_LISTBOX.Items.Refresh();
        }

        private bool CheckForConfigFile()
        {
            return File.Exists(WorkingPath + "\\" + ConfigFileName);
        }

        private void CreateConfigFile(string pathToFile)
        {
            ResultConsole.AddConsoleLine("I'm pretending to generate a new config file!");
        }

        private void OnUpdateConsole()
        {
            RESULTS_BOX.Text = ResultConsole.ConsoleString;
            RESULTS_BOX.ScrollToEnd();
        }

        private void RUN_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            var si = (Andromeda.Command.Action)AVAIL_ACTS_LISTBOX.SelectedItem;
            try
            {
                string msg = si.RunCommand("warmachine");
                ResultConsole.AddConsoleLine(msg);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("No command selected. Please select an action to take on the selected machines. \n" + ex.Message);
            }
        }

        private void InitializeConsole()
        {
            ResultConsole.InitializeResultConsole();
            ResultConsole.ConsoleChange += OnUpdateConsole;
        }


    }
}
