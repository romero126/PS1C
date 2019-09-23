#Write-Host "Setting Verbose Preference"
#$VerbosePreference = "Continue"



# Write-Host "Default baseline tests from Microsoft Powershell Team" -ForegroundColor Cyan
# These are Default Tests powershell uses modified for PS1C


#Invoke-Pester Tests\Get-ChildItem.Tests.ps1
#Invoke-Pester Tests\Get-Item.Tests.ps1
#Invoke-Pester Tests\New-Item.Tests.ps1
#Invoke-Pester Tests\Remove-Item.Tests.ps1
Invoke-Pester .\Tests\Clear-Content.Tests.ps1
Invoke-Pester Tests\Get-Content.Tests.ps1
Invoke-Pester Tests\Set-Content.Tests.ps1

#Write-Host "Internal Tests" -ForegroundColor Cyan
#Invoke-Pester Tests\PS1C.Example.Tests.ps1


