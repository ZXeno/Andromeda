using System;
using System.Collections.Generic;
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
using Andromeda.MVVM;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AboutWindow aboutWindow;

        public MainWindow()
        {
            InitializeComponent();
            
            CREDS_LABEL.Content = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);

        }

        public void UpdateCommandsListbox(List<Andromeda.Command.Action> commands)
        {
            AVAIL_ACTS_LISTBOX.ItemsSource = commands;
            AVAIL_ACTS_LISTBOX.Items.Refresh();
        }

        public void OnUpdateConsole()
        {
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + this.ToString() + " is where the error is located. \n " + ex.TargetSite.ToString());
                
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

        

        private void CHANGE_CREDS_BTTN_Click(object sender, RoutedEventArgs e)
        {
            CredentialWindow credsWindow = new CredentialWindow();
            credsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            credsWindow.Owner = App.Current.MainWindow;
            credsWindow.ShowDialog();
        }

        private void ActionListChange()
        {
            //PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            //view.GroupDescriptions.Add(groupDescription);
            //AVAIL_ACTS_LISTBOX.Items.Refresh();
        }
    }
}
