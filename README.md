# power-apps-portal-file-sync
Power Apps Portal File Sync

This tool helps you to download and upload the website structure of Power App Portals for local file editing e.g. in Visual Studio Code. 

Supported Features:
- WebFiles
- WebPages
- WebTemplates
- ContentSnippets

Run this tool from the commandline with the following parameters:

E.g. 
To download:
 .\PowerAppsPortalsFileSync.exe -u https://[org].[region].dynamics.com/ -e [admin]@[org].onmicrosoft.com -p [password] -f [Path to local folder]
 
 To upload append -s:
  .\PowerAppsPortalsFileSync.exe -u https://[org].[region].dynamics.com/ -e [admin]@[org].onmicrosoft.com -p [password] -f [Path to local folder] -s
  
  
  
  
