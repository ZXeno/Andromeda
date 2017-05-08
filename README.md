# README #

The program relies on external products for some actions to work (namely "Install TightVNC" and anything based on PSExec), and Active Directory for user authentication. These products are not, and cannot, be included in this repository. However, after you acquire them (they are free downloads), store them in a directory of your choosing, and point the configuration file for the program at them. To change the compile-time defaults of where your components directory is, open up ConfigManager.cs is Andromeda-Actions-Core project and change the field value for "_componentsDirectory" to any file location you wish to keep program components. This can also be changed after the program is launched by editing the configuration file in C:\users\{username}\Documents\Andromeda\config.dat

### Quick Summary ###

* Program written in Visual Studio 2015 w/ Resharper
* This application is meant to help automate various tasks for IT Desktop Analysts
* Current version: .7
 

### Required External Components ###
* PSExec (http://bit.ly/1GxtZ6y)

### How do I get set up? ###
* Compile the program and run the msi in the Release directory created during a Release build.
* Ensure proper components are available in the Components directory after install (PSExec)
* Launch exectuable.
 
Compiling the installer:
* Andromeda now uses the Installer project available from the "Visual Studio Marketplace". It can be found here: https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.MicrosoftVisualStudio2015InstallerProjects

