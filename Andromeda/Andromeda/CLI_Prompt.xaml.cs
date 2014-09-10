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
using System.Windows.Shapes;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for CLI_Prompt.xaml
    /// </summary>
    public partial class CLI_Prompt : Window
    {
        private string boxContents;
        private bool wasCanceled = false;

        public string TextBoxContents { get { return boxContents; } }
        public bool WasCanceled { get { return wasCanceled; } }

        public CLI_Prompt()
        {
            InitializeComponent();
            Closed += CLI_Prompt_Closed;
        }

        void CLI_Prompt_Closed(object sender, EventArgs e)
        {
            wasCanceled = true;
            this.Close();
        }

        public void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            wasCanceled = true;
            this.Close();
        }

        private void Okay_Button_Click(object sender, RoutedEventArgs e)
        {
            boxContents = CommandBox.Text;
            wasCanceled = false;
            this.Close();
        }

        private void CommandBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            boxContents = CommandBox.Text;
        }
    }
}
