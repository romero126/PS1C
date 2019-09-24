

<#
#Compile RESX
$Files = Get-ChildItem -path $PSSCriptRoot\Source\*.resx -Recurse

foreach ($File in $Files)
{
    $File
    [xml]$Resx = Get-Content $File

    $OutString = @"
using System;
using System.Collections.Generic;

namespace PS1C
{
    public static class $($File.BaseName)
    {
$(
        foreach ($Element in $Resx.Root.Data)
        {
            "`n       internal static string {0,-100} =   `"{1}`";" -f $Element.Name, $Element.Value.Replace('"', '\"j')
        }
)

    }
}

"@


    #$OutString
    $OutFile = Join-Path $File.Directory "$($File.BaseName).cs"
    $OutString | Out-File $OutFile
}


#>

function Pull-File
{
    param(
        [String]$URI,
        [Switch]$Raw
    )
    $BaseURI = "https://raw.githubusercontent.com/PowerShell/PowerShell/master/src/"

    $FullURI = "${BaseURI}/${URI}"

    $ExportPath = Join-Path "${PSScriptRoot}" "/Source/PS1C/Powershell/${URI}"

    $ExportFolder = [System.IO.Path]::GetDirectoryName($ExportPath)
    New-Item -Path $ExportFolder -ItemType Directory -Force | Out-Null

    Write-Host "Downloading Content", $URI -ForeGroundColor Cyan

    Invoke-WebRequest -uri $FullURI -UseBasicParsing | % Content | Out-File -FilePath $ExportPath


}

#Pulled Files
#Pull-File -URI "System.Management.Automation/namespaces/FileSystemContentStream.cs"
#Pull-File -URI "System.Management.Automation/utils/StructuredTraceSource.cs"
#Pull-File -URI "System.Management.Automation/utils/PInvokeDllNames.cs"
#Pull-File -URI "System.Management.Automation/utils/StringUtil.cs"
#Pull-File -URI "System.Management.Automation/utils/MshTraceSource.cs"
#Pull-File -URI "System.Management.Automation/resources/AutomationExceptions.resx"
#Pull-File -URI "System.Management.Automation/resources/FileSystemProviderStrings.resx"
#Pull-File -URI "System.Management.Automation/utils/EncodingUtils.cs"
#Pull-File -URI "System.Management.Automation/resources/PathUtilsStrings.resx"
#Pull-File -URI "System.Management.Automation/utils/assert.cs"
#Pull-File -URI "System.Management.Automation/namespaces/FileSystemProvider.cs"
#Pull-File -URI "System.Management.Automation/utils/ClrFacade.cs"
#Pull-File -URI "System.Management.Automation/CoreCLR/CorePsPlatform.cs"
#Pull-File -URI "System.Management.Automation/engine/Attributes.cs"
#Pull-File -uri "System.Management.Automation/engine/Utils.cs"

Write-Host ""
Write-Host "Building"
dotnet build .\Source\PS1C\ -v q | Select-String "Error" | Select -unique | Write-Host -ForegroundColor Red

#pwsh.exe -Command ". { .\Test.ps1 }"

#Start-Process pwsh.exe -ArgumentList { -NoExit -Command ". { .\test.ps1 }" -i }

#$PSPath = "pwsh.exe"
#$PSPath = "C:\Program Files\PowerShell\6\pwsh.exe"
$PSPath = "C:\Program Files\PowerShell\7-preview\pwsh.exe"

#$PSPath = "C:\Program Files\PowerShell\6\pwsh.exe"
#& $PSPath -Command ". .\test.ps1"

#Start-Process $PSPath -ArgumentList { -NoExit -Command ". { .\test.ps1 }" } -Wait