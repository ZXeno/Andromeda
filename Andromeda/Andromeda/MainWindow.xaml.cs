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
                    si.RunCommand(DEVICE_LIST_TEXTBOX.Text);
                }
                else
                {
                    MessageBox.Show("No action selected. Please select an action to take on the selected machines. \n\n I'm just a dumb machine, you have to tell me what to do!");
                }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message + "\n The application will now close.");
                Application.Current.Shutdown();
            }
        }

        // Open about window
        private void ABOUT_BTTN_Click(object sender, RoutedEventArgs e)
        {
            aboutWindow = new AboutWindow();
            aboutWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        public void ImportConfiguration()
        {
            string p = WorkingPath + "\\" + ConfigFileName;
            if (CheckForConfigFile())
            {
                try 
                { 
                    ConfigFile = XMLImport.GetXMLFileData(p);
                    if (ConfigFile != null)
                    {
                        configuration.UpdateConfigDocument(ConfigFile);
                    }
                    else
                    {
                        configuration = new Config(p);
                    }
                }
                catch (FileNotFoundException fnf)
                {
                    MessageBox.Show("File unable to load. \n Exception: " + fnf.Message);
                    App.Current.Shutdown();
                }

                if (ConfigFile != null)
                {
                    ResultConsole.AddConsoleLine("Configuration file found.");
                    ResultConsole.AddConsoleLine("Config file loaded. - " + p);
                }
            }
            else
            {
                ResultConsole.AddConsoleLine("No config file found!");
                CreateConfigFile(p);
            }
        }

        public void ImportCommands()
        {
            Commander = new Commands();
            AVAIL_ACTS_LISTBOX.ItemsSource = Commander.ActionsList;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(AVAIL_ACTS_LISTBOX.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            view.GroupDescriptions.Add(groupDescription);
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
            ResultConsole.AddConsoleLine("Generating new config file...");
            configuration = new Config(WorkingPath + "\\" + ConfigFileName);
            if (CheckForConfigFile())
            {
                ResultConsole.AddConsoleLine("Config file created.");
            }
            else
            {
                ResultConsole.AddConsoleLine("For some reason, the config file location either isn't readable, \n or there was another problem generating the configuration file.");
                ResultConsole.AddConsoleLine("For now, I'm not sure what to do with this, so we'll use a default configuration for the time being.");
            }
        }

        public void InitializeBackEnd()
        {
            ResultConsole.InitializeResultConsole();
            
        }

        private void CHANGE_CREDS_BTTN_Click(object sender, RoutedEventArgs e)
        {
            CredentialWindow credsWindow = new CredentialWindow();
            credsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            credsWindow.Owner = App.Current.MainWindow;
            credsWindow.ShowDialog();
        }
    }
}
