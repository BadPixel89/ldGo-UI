# ldGo-UI - Link Discovery for Windows 11

ldGo-UI is a next generation link discovery tool, fully compatible with Windows 11 in Enterprise environments. In some environments elevation may be required, but in others it may not. This is dependent on the policies in place at your organisation, on a vanilla install of Windows 11 I have not seen a UAC prompt to run the software. 

![image](https://github.com/user-attachments/assets/63531974-447f-4911-9294-12710b2ffa4b)

For those who prefer CLI tools, the internal executable module "ldGo" can be run independently from the command line, with easy to understand flags. You can find this executable in the same folder as the UI wrapper after first run. 

Once you have located it run .\gold.exe -h for a full helptext.

For more info and cross platform versions see the ldGo repo [here](https://github.com/BadPixel89/ldGo)

# This application depends on npcap, please install it before running ldGo-UI. 
You will need admin/elevation to install this

[npcap](https://npcap.com/#download)

# What is this for?

Link discovery is a method by which network switches announce certain information about themselves. This includes the curently connected port/interface, VLAN ID, and switch name. 

This info can save a lot of time tracing cables to find out which port is connected to a given PC in an office or lab environment. 

ldGo-UI supports both Cisco Discovery Protocol (CDP) and Link Layer Discovery Protocol (LLDP).

# Beta release
Release page:

https://github.com/BadPixel89/ldGo-UI/releases/tag/v0.1b

Direct link:


