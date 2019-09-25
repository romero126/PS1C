#Write-Host "Setting Verbose Preference"
#$VerbosePreference = "Continue"



Write-Host "Default baseline tests from Microsoft Powershell Team" -ForegroundColor Cyan
# These are Default Tests powershell uses modified for PS1C

#Invoke-Pester "Tests\New-Item.Tests.ps1"
#return
$Tests = @(
    # "Tests\Get-ChildItem.Tests.ps1"
    
    "Tests\Add-Content.Tests.ps1"
    "Tests\Clear-Content.Tests.ps1"
    "Tests\Get-Content.Tests.ps1"
    "Tests\Set-Content.Tests.ps1"
    
    # "Tests\Clear-Item.Tests.ps1"
    # "Tests\Copy-Item.Tests.ps1"
    # "Tests\Get-Item.Tests.ps1"
    # "Tests\Invoke-Item.Tests.ps1"
    # "Tests\Move-Item.Tests.ps1"
    "Tests\New-Item.Tests.ps1"
    # "Tests\Remove-Item.Tests.ps1"
    # "Tests\Rename-Item.Tests.ps1"
    # "Tests\Set-Item.Tests.ps1"
    # "Tests\Get-Location.Tests.ps1"
    # "Tests\Pop-Location.Tests.ps1"
    # "Tests\Push-Location.Tests.ps1"
    # "Tests\Set-Location.Tests.ps1"
    # "Tests\Join-Path.Tests.ps1"
    # "Tests\Convert-Path.Tests.ps1"
    # "Tests\Split-Path.Tests.ps1"
    # "Tests\Resolve-Path.Tests.ps1"
    # "Tests\Test-Path.Tests.ps1"
    # "Tests\New-PSDrive.Tests.ps1"
    # "Tests\Get-PSDrive.Tests.ps1"
    # "Tests\Remove-PSDrive.Tests.ps1"
    # "Tests\Get-PSProvider.Tests.ps1"
)
# 4/23 Tests
Invoke-Pester -Script $Tests

Write-Host "Tests Created:", "$($Tests.Count)/23"