# README #



To use this application: simply compile and run the executable. 

To change the directory that updates are searched for, in Program.cs change the line: 
public static string DirectoryToCheckForUpdate = "\\\\melvin\\Andromeda\\";
To whatever location you wish to keep updated versions.

The program relies on external products for some actions to work (namely "Install TightVNC" and anything based on PSExec). These products are not, and cannot, be included in this repository. However, after you acquire them (they are free downloads), store them in a directory of your choosing, and point the configuration file for the program at them. To change the compile-time defaults of where your components directory is, open up ConfigManager.cs and change the field value for "_componentsDirectory" to any file location you wish to keep program components.

### Quick Summary ###

* Program written in Visual Studio 2015 w/ Resharper
* This application is meant to help automate various tasks for IT Desktop Analysts
* Current version: .2

### How do I get set up? ###

* Ensure proper components are available (You will need at least PSExec, and if you plan on using the TightVNC installer functionality, an installer for that program)
* Either configure the embedded defaults for the configuration, or modify the runtime generated configuration file, usually located in C:\users\<user>\Documents\Andromeda
* Recommended location to keep the executable is in a folder in %USERPROFILE%\Documents\Andromeda directory, as this is the default directory for the configuration, results, and log files for the program. 