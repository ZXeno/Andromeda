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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public string AboutText { get; protected set; }
        public AboutWindow()
        {
            InitializeComponent();
            ConstructContent();
        }

        private void ConstructContent()
        {
            AboutText = "This software was written by Jonathan B. Cain. \n Desktop Analyst, Peacehealth Oregon - Riverbend Hospital \n \n I want to thank: Brian Lambert, formerly of PHO-RB Desktop Team, for his insight and knowledge. The PHO Desktop Team for their support and collective knowledge. \n And I'd like to thank Carl Sagan and Neil DeGrasse Tyson for their passion and spread of knowledge to beautiful young minds across several generations. Without them, inspiration for learning these skills may have never happened!";

            this.ABOUT_LABEL.Text = AboutText;
            this.ABOUT_LABEL.TextAlignment = TextAlignment.Left;
            this.ABOUT_LABEL.TextWrapping = TextWrapping.Wrap;
            
        }

        private void Andromeda_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/Andromeda_Galaxy");
        }

        private void STEM_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.futurefocusfoundation.us/donate/");
        }

        private void Cosmos_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.amazon.com/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords=Cosmos");
        }
    }
}
