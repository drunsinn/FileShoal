#FileShoal
With FileShoal you can copy files to a number of different systems via ssh/scp. The main purpose is for distributing Scripts and configuration files to many systems at once. It can also be used to back-up said files from the machines to your local drive.
The transfer is handled by the [WinSCP Library for C#](https://winscp.net/eng/docs/library)
Authentication is done via SSH keys. These can be generated with PuTTYgen. Please refert to the [PuTTY documentation](https://the.earth.li/~sgtatham/putty/0.71/htmldoc/Chapter8.html#pubkey) for a detailed explanation on how to generate SSH keys and how to use them.
After transferring the private key to each target machine and adding it to your configuration. You will have to add the host key of the target machine to the machine configuration file (machines.xml).
 
Please consider donaitiong to to the WinSCP and PuTTY projects for there efforts!
 
![FileShoal main window](/screenshot.png)
 
## Requirements
FileShoal is a 64bit application and requires Microsoft .NET Framework 4.6 or later to be present on the machine. WinSCP and the WinSCP library are included in the setup package.
 
## Download
Releases can be found [here](https://github.com/drunsinn/FileShoal/releases)
 
## Configuration
FileShoal uses two configuration files
 
### App.config / FileShoal.exe.config
This file contains the application settings. It is located in the application directory.
 
You can configure connection settings, remote paths as well as the path to the list with the remote machines.
 
```
MachineConfigPath  <---- path to the xml-file containing the connection information
RemotePath  <---- folder name on the remote machines
RemoteBasePath  <---- path on the remote machine
RemoteUserName  <---- user name on the remote machine
PrivateKeyPath  <---- path to the private key file used to connect to the remote machines
```
 
### machines.xml
This xml-file contains all the information regarding the remote machines. It can be saved anywhere you want, just set the configuration value MachineConfigPath to the correct value.
 
#### Example
```
<?xml version="1.0"?>
<ArrayOfMachineConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <MachineConfiguration>
    <Name>Machine_I</Name>  <---- display name of the machine
    <Address>192.168.0.50</Address>  <---- IP or hostname
    <Fingerprint>ssh-rsa 2048 ABCDE/ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890=</Fingerprint>  <---- Remote Fingerprint
    <ActiveTarget>true</ActiveTarget>  <---- was machine used for the last transfer
  </MachineConfiguration>
</ArrayOfMachineConfiguration>
```
 
## ToDo
* Create default configuration file if it does not exist (machines.xml)
* Open connections to different machines in parallel
* Design application icon
* Create binary release
* Add better documentation
