using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for CredentialWindow.xaml
    /// </summary>
    public partial class CredentialWindow : Window
    {
        private bool userCheckedOkay = false;
        private bool passCheckedOkay = false;
        private bool credsWereChecked = false;

        public CredentialWindow()
        {
            InitializeComponent();
        }

        private void Okay_Button_Click(object sender, RoutedEventArgs e)
        {
            if (credsWereChecked)
            {
                if (userCheckedOkay && passCheckedOkay)
                {

                }
                else if (userCheckedOkay && !passCheckedOkay)
                {
                    MessageBox.Show("Your credentials failed to validate, please try again.");
                }
                else if (!userCheckedOkay)
                {
                    MessageBox.Show("Your username was not found in AD.");
                }
                else
                {
                    MessageBox.Show("I'm not completely sure what went wrong. \n\n Please try re-entering your credentials.");
                }
            }
            else
            {
                if (CredentialManager.DoesUserExistInActiveDirectory(ufield.Text))
                {

                }
            }
        }
    }
}
