#Import-Module ..\..\..\ps1c.psm1

$path = (Get-Item $PSScriptRoot\..\PS1C\).FullName

if ($env:PSModulePath -notlike "*$path*") {
    $env:PSModulePath += ";$path"
}

Write-Host "Importing module PS1C"
Import-Module $path\PS1C.psd1 -Force

# Pester Tests
$tests = @(
    "$PSScriptRoot\PS1C\Add-Content.Tests.ps1",    # Successful
    "$PSScriptRoot\PS1C\Clear-Content.Tests.ps1",  # Successful
    "$PSScriptRoot\PS1C\Get-Content.Tests.ps1",    # Successful / Disabled UTF7 support
        # A warning should be emitted if both -AsByteStream and -Encoding are used together

    "$PSScriptRoot\PS1C\New-Item.Tests.ps1",       # Successful
    "$PSScriptRoot\PS1C\Remove-Item.Tests.ps1"     # Successful
        # Rename-Item will rename a file when path contains special char
    "$PSScriptRoot\PS1c\Rename-Item.Tests.ps1",    # Successful
    "$PSScriptRoot\PS1C\Set-Content.Tests.ps1"     # Successful
        # All of Set-Content should create a file if it does not exist without -Force option
)

Invoke-Pester -Script $tests -Output Detailed -PassThru

Remove-Module PS1C