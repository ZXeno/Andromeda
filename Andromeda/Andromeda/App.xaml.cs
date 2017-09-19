using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Andromeda.View;
using Andromeda.ViewModel;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore;
using AndromedaCore.Plugins;
using AndromedaCore.ViewModel;

namespace Andromeda
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string VersionNumber = "Version ";

        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
        public static string PluginFolder = WorkingPath + "\\Plugins";
        public static IoCContainer IoC { get; private set; }

        private ILoggerService _logger;
        private CredentialManager _credman;
        private ResultConsole _resultConsole;
        private ActionFactory _actionFactory;

        public static ConfigManager ConfigManager { get; private set; }
        public static ActionManager ActionManager { get; private set; }
        public static PluginManager PluginManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionHandler;

            VersionNumber += Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
#if DEBUG
            UserFolder = WorkingPath;
#endif

            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            InitializeIoCContainer();

            _logger = IoC.Resolve<ILoggerService>();

            _credman = new CredentialManager();
            _resultConsole = new ResultConsole();
            ConfigManager = new ConfigManager(UserFolder, IoC.Resolve<IXmlServices>(), IoC.Resolve<ILoggerService>());
            ActionManager = new ActionManager(_logger);
            PluginManager = new PluginManager(_logger);
            _actionFactory = new ActionFactory(IoC);

            // Load Core actions
            LoadCoreActions();

            // Load and initialize all plugins from Plugins folder
            PluginManager.LoadAllPlugins();
            PluginManager.InitializeAllPlugins();
            
            // set up login window
            var loginWindow = new LoginWindow();
            var loginWindowViewModel = new LoginWindowViewModel
            {
                SuccessAction = () => loginWindow.DialogResult = true,
                CancelAction = () => loginWindow.DialogResult = false
            };
            loginWindow.DataContext = loginWindowViewModel;
            
            // Initialize Main Window
            var mainWindowViewModel = new MainWindowViewModel(IoC.Resolve<ILoggerService>(),IoC.Resolve<IWindowService>(), ActionManager);
            mainWindowViewModel.LoadActionsCollection();
            var window = new MainWindow
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

            mainWindowViewModel.UpdateLoginProperties();

            // show main window
            window.Show();

            Application.Current.MainWindow = window;
        }

        private void InitializeIoCContainer()
        {
            IoC = new IoCContainer();

            IoC.Register<ILoggerService, Logger>(LifeTimeOptions.ContainerControlledLifeTimeOption);
            IoC.Register<IFileAndFolderServices, FileAndFolderServices>();
            IoC.Register<INetworkServices, NetworkServices>();
            IoC.Register<IPsExecServices, PsExecServices>();
            IoC.Register<IWmiServices, WmiServices>();
            IoC.Register<ISccmClientServices, SccmClientServices>();
            IoC.Register<IXmlServices, XmlServices>();
            IoC.Register<IRegistryServices, RegistryServices>();
            IoC.Register<IWindowService, WindowService>();

        }

        private void LoadCoreActions()
        {
            _logger.LogMessage("Loading core Andromeda actions...");

            // Dynamically get all of our core action classes and load them.
            var @corenamespace = "AndromedaActions.Command";
            var assembly = Assembly.LoadFile(WorkingPath + "\\AndromedaActions.dll");
            var q = from t in assembly.GetTypes() where t.IsClass && t.Namespace == @corenamespace select t;

            var instantiatedCoreActions = ActionFactory.InstantiateAction(q);
            ActionManager.AddActions(instantiatedCoreActions);
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            _logger.LogError($"Unhandled Exception", args.ExceptionObject as Exception);
            MessageBox.Show(Application.Current.MainWindow, $"Andromeda has encountered an unhandled exception. See the log file for more information. Please restart Andromeda.");
            Application.Current.Shutdown();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            //TODO: Add cleanup of all application resources

            base.OnExit(e);
        }
    }
}