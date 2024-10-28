#Import-Module ..\..\..\ps1c.psm1

$path = (Get-Item $PSScriptRoot\..\PS1C\).FullName

if ($env:PSModulePath -notlike "*$path*") {
    $env:PSModulePath += ";$path"
}

Write-Host "Importing module PS1C"
Import-Module $path\PS1C.psd1 -Force

# Pester Tests
$tests = @(
    "$PSScriptRoot\PS1C\Add-Content.Tests.ps1",   # Successful
    "$PSScriptRoot\PS1C\Clear-Content.Tests.ps1", # Fail 2/9 Tests (Errors should throw on bad locations)
    "$PSScriptRoot\PS1C\Get-Content.Tests.ps1"   # Fail 2/41 Tests ( multi-byte chars )
    "$PSScriptRoot\PS1C\New-Item.Tests.ps1",      # Successful
    "$PSScriptRoot\PS1C\Remove-Item.Tests.ps1",   # Fail 2 tests on regex
    "$PSScriptRoot\PS1c\Rename-Item.Tests.ps1",   # Successful
    "$PSScriptRoot\PS1C\Set-Content.Tests.ps1"    # Successful
)

Invoke-Pester -Script $tests -Output Detailed -PassThru