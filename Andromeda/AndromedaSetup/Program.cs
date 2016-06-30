using System;
using System.Linq;
using System.Security.Principal;

namespace AndromedaSetup
{
    internal class Program
    {
        public static string InstallLogFileName { get; private set; }
        public static string InstallLogPath { get; private set; }

        private static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("You must have administrative rights to run this installer.");
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
                return;
            }

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
            InstallLogFileName = "Install_log.ail";
            InstallLogPath = "C:\\Windows\\Installer\\AndromedaUninstaller";
            var installer = new Installer();

            if (args.Contains("/?") || args.Contains(" /help"))
            {
                //TODO: Spit out help info
            }

            if (args.Contains("/uninstall"))
            {
                installer.Uninstall();
                return;
            }

            if (args.Contains("/quiet"))
            {
                // TODO: Implement silent install
            }

            if (args.Contains("/path="))
            {
                path = args.Select(x => x.Contains("/path=")).ToString().Remove(0, 5);
                InstallLogPath = path;
            }
            
            installer.Install(path);
            
            Console.WriteLine("Setup complete. Press ENTER to exit.");
            Console.ReadLine();
        }

        private static void PrintHelpText()
        {
            
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}