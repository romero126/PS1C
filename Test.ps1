Write-Host "Setting Verbose Preference"
$VerbosePreference = "Continue"

#Personal Tests
Import-Module .\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.dll -Verbose;
new-psdrive -name APPX -psprovider PS1C -root "$PSScriptRoot\Tests\ZipFile.zip" -Verbose
Get-PSDrive APPX | fl
cd APPX:
Get-ChildItem | ft -a
$Message = "{0} : {1}" -f (Get-Date), (New-Guid)
$Message | Set-Content .\Test.txt
Write-Host "Writing Content '${Message}'" -ForegroundColor Cyan
#Append only currently
Get-Content .\Test.txt | select -last 5 | Write-Host -ForegroundColor Yellow

return































#Get-Item APPX:\Test.txt
#Get-Item .\Test.txt

#$Bytes = Get-Content .\Test.txt -AsByteStream -Raw
<#
$FileInfo = Get-ChildItem .\Test.txt 
$FileStream = $FileInfo.Open([System.IO.FileMode]::Append)
#$FileStream

$StreamWriter = [System.IO.StreamWriter]::new($FileStream)
$Value = "Hello World"
$StreamWriter.WriteLine($value)
$StreamWriter.Flush() | out-null


$FileStream.Close()
$FileStream.Dispose()
#>

#$FileStream = $FileInfo.Open([System.IO.FileMode]::Append)
#$StreamReader = [System.IO.StreamReader]::new($FileStream)
#
#Write-Host $StreamReader.ReadToEnd() -ForegroundColor Yellow

#$StreamReader.Close()
#$FileStream.Close()
#$FileStream.Dispose()























return

Invoke-Pester .\Tests\
<#
Import-Module .\Source\PS1C\bin\Debug\netstandard2.0\ps1c.dll -Verbose;
new-psdrive -name APPX -psprovider PS1C -root "$($pwd.path)\ZipFile.zip" -Verbose
Get-PSDrive APPX | fl
cd APPX:
#>
