using System;
using System.Windows;
using System.Xml;
using Andromeda.ViewModel;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Program Program { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            Program = new Program();

            MainWindow window = new MainWindow();
            var viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;
            window.Title = "Andromeda";
            window.Height = 750;
            window.Width = 800;
            window.ResizeMode= ResizeMode.CanMinimize;
            window.Show();
        }
    }
}
