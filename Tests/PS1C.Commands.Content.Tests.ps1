describe "PS1C PSDrive" {
    beforeall {
        $ArchiveFile = "$PSScriptRoot\ZipFile"
        Import-Module .\Source\PS1C\bin\Debug\netstandard2.0\ps1c.dll
        New-PSDrive -Name PesterTests -PSProvider PS1C -root "$ArchiveFile.zip" -ErrorAction "Stop"
    }

    context "Get-Content" {
        it "[NEG] Non Existant File" {
            Get-Content 
        }
        it "[POS] On Existing File" {

        }

    }
    context "Set-Content" {
        it "Get" {

        }
    }

    context "Remove-PSDrive" {

    }

}
