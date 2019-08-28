#Write-Host "Setting Verbose Preference"
#$VerbosePreference = "Continue"



$ArchiveFile = "$PSScriptRoot\Tests\ZipFile"
#Import-Module $PSScriptRoot\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.psd1 -Force
#New-PSDrive -Name PSProvider -PSProvider ZipFile -root "$ArchiveFile.zip" -ErrorAction "Stop"
#
#Get-Item PSProvider:\Test.txt
#
#Get-Item PSProvider:\Waffles
#
#Remove-PSDrive PSProvider

Write-Host "Default baseline tests from Microsoft Powershell Team" -ForegroundColor Cyan
# These are Default Tests powershell uses modified for PS1C
# Invoke-Pester Tests\PS1C.Get-Content.Tests.ps1
Invoke-Pester Tests\PS1C.Set-Content.Tests.ps1

#Write-Host "Internal Tests" -ForegroundColor Cyan
#Invoke-Pester Tests\PS1C.Example.Tests.ps1


