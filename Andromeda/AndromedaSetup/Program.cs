using System;
using System.Linq;
using System.Security.Principal;

namespace AndromedaSetup
{
    internal class Program
    {
        public static string InstallLogFileName { get; private set; }
        public static string InstallLogPath { get; private set; }
        public static bool IsSilentInstall { get; private set; }

        private static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("You must have administrative rights to run this installer.");
                Console.WriteLine("Press ENTER to exit.");
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].ToLower();
            }

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
            InstallLogFileName = "Install_log.ail";
            InstallLogPath = "C:\\Windows\\Installer\\AndromedaUninstaller";
            var installer = new Installer();

            if (args.Contains("/?") || args.Contains("/help"))
            {
                PrintHelpText();
                return;
            }

            if (args.Contains("/uninstall"))
            {
                installer.Uninstall();
                return;
            }

            if (args.Contains("/quiet"))
            {
                IsSilentInstall = true;
            }

            if (args.Contains("/path="))
            {
                path = args.Select(x => x.Contains("/path=")).ToString().Remove(0, 5);
                InstallLogPath = path;
            }
            
            installer.Install(path);

            if (!IsSilentInstall)
            {
                Console.WriteLine("Setup complete. Press ENTER to exit.");
                Console.ReadLine();
            }
        }

        private static void PrintHelpText()
        {
            Console.WriteLine("----ANDROMEDA INSTALLER HELP----");
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            Console.WriteLine("/? OR /help     Prints this help informaiton.");
            Console.WriteLine("/path=<newpath> Sets the destination path for installation.");
            Console.WriteLine("                Default install location is %userprofile%\\Documents");
            Console.WriteLine("/uninstall      Uninstalls Andromeda from this system.");
            Console.WriteLine("/quiet          Performs a silent install");
            Console.WriteLine();
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}