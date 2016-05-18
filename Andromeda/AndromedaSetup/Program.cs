using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AndromedaSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = new InstallConfig();
                if (!config.LoadConfigFile(Environment.CurrentDirectory + "\\setup.dat"))
                {
                    throw new Exception("Unable to load setup.dat.");
                }

                foreach (var process in Process.GetProcessesByName("Andromeda"))
                {
                    Console.WriteLine("Ending process " + process.ProcessName);
                    process.Kill();
                }

                Thread.Sleep(1000);

                var expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Andromeda";
                if (Directory.Exists(expectedPath))
                {
                    Directory.Delete(expectedPath, true);
                }


                config.InstallableItems.ForEach(item =>
                {
                    if (!File.Exists(item.Source))
                    {
                        throw new Exception("Could not find source item " + item.Item);
                    }

                    if (!Directory.Exists(item.Destination))
                    {
                        Directory.CreateDirectory(item.Destination);
                    }

                    var fileName = item.Source.Split(new char[] { '\\' }).Last();

                    File.Copy(item.Source, item.Destination + "\\" + fileName, true);
                    Console.WriteLine("Copied " + fileName);
                });
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
                return;
            }

            if (args.Contains("/q")) return;

            Console.WriteLine("Setup complete. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}