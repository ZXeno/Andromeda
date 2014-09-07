using System;
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

        public static Config configuration;
        public static Commands Commander { get; set; }

        private AboutWindow aboutWindow;



        public MainWindow()
        {
            InitializeComponent();
            InitializeConsole();
            ImportConfiguration();
            ImportCommands();
            CREDS_LABEL.Content = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
        }

        public void UpdateCommandsListbox(List<Andromeda.Command.Action> commands)
        {
            AVAIL_ACTS_LISTBOX.ItemsSource = commands;
            AVAIL_ACTS_LISTBOX.Items.Refresh();
        }

        public void OnUpdateConsole()
        {
            RESULTS_BOX.Text = ResultConsole.ConsoleString;
            RESULTS_BOX.ScrollToEnd();
        }

        // Run our selected command.
        private void RUN_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            var si = (Andromeda.Command.Action)AVAIL_ACTS_LISTBOX.SelectedItem;

            try
            {
                if (si != null)
                {
                    string msg = si.RunCommand(DEVICE_LIST_TEXTBOX.Text);
                    ResultConsole.AddConsoleLine(msg);
                }
                else
                {
                    MessageBox.Show("No action selected. Please select an action to take on the selected machines. \n What do you expect to do with these, otherwise?");
                }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message + "\n The application will now close.");
                Application.Current.Shutdown();
            }
        }

        private void ABOUT_BTTN_Click(object sender, RoutedEventArgs e)
        {
            if (aboutWindow == null) { aboutWindow = new AboutWindow(); }
            aboutWindow.Show();
        }

        public void ImportConfiguration()
        {
            string p = WorkingPath + "\\" + ConfigFileName;
            if (CheckForConfigFile())
            {
                try { ConfigFile = XMLImport.GetXMLFileData(p); }
                catch (FileNotFoundException fnf)
                {
                    MessageBox.Show("File unable to load. \n Exception: " + fnf.Message);
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

        private void InitializeConsole()
        {
            ResultConsole.InitializeResultConsole();
            ResultConsole.ConsoleChange += OnUpdateConsole;
        }

        private bool CheckForConfigFile()
        {
            return File.Exists(WorkingPath + "\\" + ConfigFileName);
        }

        private void CreateConfigFile(string pathToFile)
        {
            ResultConsole.AddConsoleLine("I'm pretending to generate a new config file!");
        }

        public void InitializeBackEnd()
        {
            ResultConsole.InitializeResultConsole();
            
        }
    }
}
