# README #

The program relies on external products for some actions to work (namely "Install TightVNC" and anything based on PSExec), and Active Directory for user authentication. These products are not, and cannot, be included in this repository. However, after you acquire them (they are free downloads), store them in a directory of your choosing, and point the configuration file for the program at them. To change the compile-time defaults of where your components directory is, open up ConfigManager.cs is Andromeda-Actions-Core project and change the field value for "_componentsDirectory" to any file location you wish to keep program components. This can also be changed after the program is launched by editing the configuration file in C:\users\{username}\Documents\Andromeda\config.dat

### Quick Summary ###

* Program written in Visual Studio 2015 w/ Resharper
* This application is meant to help automate various tasks for IT Desktop Analysts
* Current version: .6
* 
### Required External Components ###
* TightVNC (http://bit.ly/1puiQ1f) rename to "tightvnc-setup-64bit.msi"
* PSExec (http://bit.ly/1GxtZ6y)

### How do I get set up? ###
* Compile the program and copy the files to your %userprofile%\Documents\Andromeda, and copy the required components to %userprofile%\Documents\Andromeda\Components.
* Ensure proper components are available in the Components directory
* Launch exectuable.
 
Compiling the installer:
* Edit file locations in setup.dat in the AndromedaSetup project
* Change the <source></source> content to match the file location (can be remote)
* Change the <destination></destination> tags to match your required destination, {user} will be replaced with the currently logged on user.

setup.dat file format:
<files>
  <file>
    <name>tightvnc-setup-64bit.msi</name>
    <source>\\PATH\TO\ANDROMEDA\FILES\Components\tightvnc-setup-64bit.msi</source>
    <destination>C:\Users\{user}\Documents\Andromeda\Components</destination>
  </file>
</files>
