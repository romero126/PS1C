Write-Host "Setting Verbose Preference"
$VerbosePreference = "Continue"

describe "ZipFileProvider" {
    beforeall {
        $ArchiveFile = "$PSScriptRoot\Tests\ZipFile"
        Import-Module .\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.dll
        New-PSDrive -Name PSProvider -PSProvider ZipFile -root "$ArchiveFile.zip" -ErrorAction "Stop"

        function Out-PesterMessage {
            param (
                [int] $indent = 2, 
                [Parameter(ValueFromPipeline)]
                [object] $InputObject
            )
            begin {
                $InputObjects = New-Object "System.Collections.Generic.List[object]"
            }
            process {
                # Collect all objects in Pipeline.
                $InputObjects.Add($InputObject)        
            }
            end {
                $OutputString = $InputObjects | 
                                                Out-String | 
                                                ForEach-Object Trim |
                                                ForEach-Object Split "`n" |
                                                ForEach-Object { "{0}{1}" -f (" " * 4 * $indent), $_ } |
                                                Write-Host -ForegroundColor Cyan
            }
        }

    }

    Context "Get-ChildItem" {
        it "[NEG] Get-ChildItem File Not Exist" {
            Get-ChildItem PSProvider:\
        }
        it "Get-ChildItem" {
            Get-ChildItem PSProvider:\ | ft LastWriteTime, Length, Name, FullName -a | Out-PesterMessage
        }
    }

    context "Get / Set Content" {
        it "Write/Read" {
            $DesiredMessage = "{0} : {1}" -f (Get-Date), (New-Guid)
            $DesiredMessage | Set-Content PSProvider:\Test.txt
            
            $ActualMessage = Get-Content PSProvider:\Test.txt

            "Desired Content: '{0}'" -f $DesiredMessage | Out-PesterMessage
            "Actual Content:  '{0}'" -f $ActualMessage | Out-PesterMessage

            $ActualMessage | Should -Be $DesiredMessage
        }
    }

    context "New/Remove-Item" {
        it "[POS] New-Item" {
            New-Item PSProvider:\Test.New-Item.txt

            Get-ChildItem PSProvider:\ | ft LastWriteTime, Length, Name, FullName -a | Out-PesterMessage
        }
        it "[NEG] New-Item Already Exists" {
            { New-Item PSProvider:\Test.New-Item.txt } | Should -Throw
        }
        it "[POS] Remove-Item" {
            Remove-Item PSProvider:\Test.New-Item.txt
            Get-ChildItem PSProvider:\ | ft LastWriteTime, Length, Name, FullName -a | Out-PesterMessage
        }
        it "[NEG] Remove-Item Doesnt Exist" {
            { Remove-Item PSProvider:\Test.New-Item.txt -ErrorAction "Stop" } | Should -Throw
        }
    }
}

return

#<#
$ArchiveFile = "$PWD\Tests\ZipFile.zip"
Import-Module .\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.dll

Write-Host $ArchiveFile -ForegroundColor CYAN

New-PSDrive -Name PSProvider -PSProvider ZipFile -root $ArchiveFile

Get-ChildItem PSProvider:\ | ft LastWriteTime, Length, Name, FullName -a

[String]$ScriptBlock = {
    Write-Host 'Hello World from Powershell'
}
$ScriptBlock | Set-Content PSProvider:\_InvokeCommand.ps1


New-Item PSProvider:\Item.txt
Get-ChildItem PSProvider:\ | ft LastWriteTime, Length, Name, FullName -a
Remove-Item PSProvider:\Item.txt
#Invoke-Item PSProvider:\_InvokeCommand.ps1

Remove-PSDrive -Name PSProvider
return


return



#Personal Tests
Import-Module .\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.dll -Verbose;
new-psdrive -name APPX -psprovider PS1C -root "$PSScriptRoot\Tests\ZipFile.zip" -Verbose
Get-PSDrive APPX | fl
cd APPX:

Get-ChildItem | ft -a #Not fully Fledged
$Message = "{0} : {1}" -f (Get-Date), (New-Guid)

Write-Host "Writing Content '${Message}'" -ForegroundColor Cyan

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
