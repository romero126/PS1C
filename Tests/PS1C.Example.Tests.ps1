describe "ZipFileProvider" {
    beforeall {
        $ArchiveFile = "$PSScriptRoot\ZipFile"
        Import-Module $PSScriptRoot\..\Source\PS1C\bin\Debug\netcoreapp3.0\ps1c.psd1 -Force
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