describe "PS1C PSDrive" {
    beforeall {
        $ArchiveFile = "$PSScriptRoot\ZipFile"
        Import-Module .\Source\PS1C\bin\Debug\netstandard2.0\ps1c.dll
    }

    context "New-PSDrive" {
        it "[POS] New-PSDrive" {
            { New-PSDrive -name PS1CN01 -psprovider PS1C -root "$ArchiveFile.zip" -ErrorAction "Stop" } | should -Not -Throw
        }
        it "[NEG] New-PSDrive: Invalid Path" {
            { New-PSDrive -name PS1CN02 -psprovider PS1C -root "$ArchiveFile.NotExists.zip" -ErrorAction "Stop" } | should -Throw
        }
        it "[NEG] New-PSDrive: Invalid Archive" {
            { New-PSDrive -name PS1CN02 -psprovider PS1C -root "$ArchiveFile.BadFile.zip" -ErrorAction "Stop" } | should -Throw
        }
    }
    context "Get-PSDrive" {
        it "Get" {

        }
    }

    context "Remove-PSDrive" {

    }

}
