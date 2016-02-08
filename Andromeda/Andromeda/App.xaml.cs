using System;
using System.Windows;
using Andromeda.View;
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
            // Being program back-end
            Program = new Program();

            // set up login window
            LoginWindow loginWindow = new LoginWindow();
            var loginWindowViewModel = new LoginWindowViewModel();
            loginWindowViewModel.SuccessAction = new Action(() => loginWindow.DialogResult = true);
            loginWindowViewModel.CancelAction = new Action(() => loginWindow.DialogResult = false);
            loginWindow.DataContext = loginWindowViewModel;
            
            // Initialize Main Window
            MainWindow window = new MainWindow();
            var viewModel = new MainWindowViewModel();
            window.DataContext = viewModel;
            window.Title = "Andromeda";
            window.Height = 750;
            window.Width = 800;
            window.ResizeMode = ResizeMode.CanMinimize;

            // Show login prompt
            loginWindow.ShowDialog();

            if (loginWindowViewModel.WasCanceled)
            {
                // program is closing if the window was canceled.
                Application.Current.Shutdown();
                return;
            }

            loginWindow = null;
            viewModel.UpdateLoginProperties();

            // show main window
            window.Show();

        }
    }
}
