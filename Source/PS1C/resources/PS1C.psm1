# Checks to see if a new update is available on Gallery
$CheckUpdate = $false
if ($CheckUpdate)
{
    $CurVersion = Get-Module -Name ps1c -ListAvailable
    $NextVersion = Find-Module -Name PS1C
    if ($CurVersion.Version -lt $NextVersion.Version)
    {
        
        Write-Host "A new version of PS1C is available." -ForegroundColor Green
        Write-Host ""
        Write-Host "Current Version:", $CurVersion.Version -ForegroundColor Yellow
        Write-Host ""

        Write-Host ("{0} Release Notes:" -f $NextVersion.Version ) -ForegroundColor Green
        Write-Host $NextVersion.ReleaseNotes
        
        Write-Host "Update-Module -Name PS1C" -ForegroundColor Yellow
    }
}
Import-Module "$PSScriptRoot\ps1c.dll"