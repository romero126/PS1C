# Welcome to PS1C a ZipFile Powershell Provider

PS1C is a Powershell Provider that Impliments basic functionality and drive access to ZipFiles.

This is by far in an Early Stages of development and will be continued to be worked on until it becomes fully Implimentable in PowerShell.

The goal for this project is to be a drag and drop implimentation into PowerShell's base code, to submit as a PR and integrated with PowerShells native codebase.

## What is PS1C?
PS1C is my first Attempt at implimenting 

# Getting Started

## Installing PS1C
``` powershell
Install-Module -Name PS1C -Force
```

## Getting Started
``` powershell
# Important Notes:
#   Path to the ZipFile must be a fully qualified path

Import-Module PS1C -Force
$ZipFile = "$PSScriptRoot\MyZipFile.zip"
New-PSDrive -Name MyZip -PSProvider ZipFile -Root $ZipFile
```

## Special Considerations

All tests of features will be pulled directly from PowerShell's CodeBase in order to Mimic most of Base Functionality of the FileSystem PSProvider with a few Exceptions.

### Currently Supported Commands

* Get-ChildItem (Early Development)
* Get-Item (Early Development)
* New-Item
* Remove-Item
* Rename-Item
* Get-Content
* Set-Content
