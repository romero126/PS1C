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


Import-Module $PSScriptRoot\build.psm1

Start-ResGen

Write-Host ""
Write-Host "Building"

Start-Build -NoWarning
