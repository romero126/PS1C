describe "PS1C FileStream Tests" {
    beforeall {
        $ArchiveFile = "$PSScriptRoot\ZipFile"
        Import-Module .\Source\PS1C\bin\Debug\netstandard2.0\ps1c.psd1 -force
    }

    context "Create" {

    }
    context "TestFile-Read" {

    }
    context "Get-PSDrive" {
        it "Get" {

        }
    }

    context "Remove-PSDrive" {
<#


$Item = Get-ChildItem | select -first 1
$ReadOnly = $Item.OpenRead()
$StreamReader = [System.IO.StreamReader]::new($ReadOnly)
$StreamReader.ReadToEnd()
$StreamReader.Dispose();
$ReadOnly.Dispose()
#>


$Item = Get-ChildItem | select -index 1
$Stream = $Item.Open([System.IO.FileMode]::Append)

$StreamWriter = [System.IO.StreamWriter]::new($Stream)
$StreamWriter.WriteLine( "Hello World $([DateTime]::Now)" )
$StreamWriter.Flush()
$Stream.Seek(0, [System.IO.SeekOrigin]::Begin);
$StreamReader = [System.IO.StreamReader]::new($Stream)

$StreamReader.ReadToEnd()

$StreamWriter.Dispose()
$StreamReader.Dispose()
$Stream.Dispose()
    }

}
