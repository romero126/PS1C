
Import-Module .\src\PS1C\bin\Debug\netcoreapp3.0\ps1c.psd1 -Force
New-PSDrive -Name PSProvider -PSProvider ZipFile -root "./Tests/ZipFile.Zip"

Get-ChildItem PSProvider:\ | ft -a