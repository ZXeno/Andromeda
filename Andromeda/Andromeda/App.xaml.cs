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
            // Begin program back-end
            Program = new Program();

            // set up login window
            var loginWindow = new LoginWindow();
            var loginWindowViewModel = new LoginWindowViewModel
            {
                SuccessAction = () => loginWindow.DialogResult = true,
                CancelAction = () => loginWindow.DialogResult = false
            };
            loginWindow.DataContext = loginWindowViewModel;
            
            // Initialize Main Window
            var mainWindowViewModel = new MainWindowViewModel();
            mainWindowViewModel.LoadActionsCollection(Program.LoadActions());
            MainWindow window = new MainWindow
            {
                Title = "Andromeda",
                Height = 750,
                Width = 800,
                ResizeMode = ResizeMode.CanMinimize,
                DataContext = mainWindowViewModel
            };
            
            // Show login prompt
            loginWindow.ShowDialog();

            if (loginWindowViewModel.WasCanceled)
            {
                // program is closing if the window was canceled.
                Application.Current.Shutdown();
                return;
            }
            
            
            mainWindowViewModel?.UpdateLoginProperties();

            // show main window
            window.Show();

        }
    }
}
