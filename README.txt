PS1C is a simple Zipfile Reader

Working Features:
dir
	Reads Directory Structure
Get-Content
	Reads content of files

TODO:
	Add Write Capability
	and Better Stream manipulation.
	
	

Example Usage:

import-module "path\PS1C.dll"
new-psdrive -name APPX -psprovider PS1C -root "$($pwd.path)\ZipFile.zip"
cd APPX:
ls | ft Name, FullName, *dir*

get-content APPX:\file.txt