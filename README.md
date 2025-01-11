# Welcome to PowerShell Compressed (PS1C) is a ZipFile Powershell Provider

## What is it?
PS1C is a PowerShell Provider that lets you mount zip/archive files as if they were native PSDrive.

## Requirements
PS1C works only in PowerShell 6 and up.

# Getting Started

## Installing PS1C
``` ps1
Install-Module -Name PS1C -Force
```

## Getting Started
``` ps1

Import-Module PS1C -Force
$ZipFile = "$PSScriptRoot\MyZipFile.zip"
New-PSDrive -Name MyZip -PSProvider ZipFile -Root $ZipFile
cd MyZip:\
```

## Currently the commandsare supported

* Add-Content
* Clear-Content
* Get-Content
* New-Item
* Remove-Item
* Rename-Item
* Set-Content

## Special Considerations
This is designed to feel as natural to PowerShell's default FileSystemprovider, however not all functionality can be implemented due to actual limitations within the PowerShell Language.

UTF7 was removed as a supported encoding format.