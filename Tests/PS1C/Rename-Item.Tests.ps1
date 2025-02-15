# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Rename-Item tests" -Tag "CI" {
    BeforeAll {
        function New-ZipFile {
            [CmdletBinding()]
            param(
                [Parameter(Mandatory)]
                [System.String] $Path
            )
            $bytes = [System.Convert]::FromBase64String("UEsFBgAAAAAAAAAAAAAAAAAAAAAAAA==")
            [System.IO.File]::WriteAllBytes($Path, $bytes)
        }

        Import-Module "$PSScriptRoot\..\..\PS1C\PS1C.psd1"

        $zipfilePath = "$env:TEMP\ZipFile.zip"
        New-ZipFile -Path $zipfilePath

        New-PSDrive -Name TestDrive -PSProvider PS1C -root "$zipfilePath" -ErrorAction "Stop"

        $TestDrive = "TestDrive:"

        $source = "$TESTDRIVE/originalFile.txt"
        $target = "$TESTDRIVE/ItemWhichHasBeenRenamed.txt"

        $sourceSp = "$TestDrive/``[orig-file``].txt"
        $targetSpName = "ItemWhichHasBeen[Renamed].txt"
        $targetSp = "$TestDrive/ItemWhichHasBeen``[Renamed``].txt"
#        Setup -Dir [test-dir]
        $wdSp = "$TestDrive/``[test-dir``]"

        # Setup file System
        New-Item $Source -Value "This is content" -Force -ErrorAction stop

        try {
            New-Item -Path $sourceSP -Value "This is not content" -Force -ErrorAction Continue
            New-Item "$wdSp" -ItemType Directory -ErrorAction Continue
        }
        catch {

        }

    }
    AfterAll {
        try {
            Remove-Item $target -Force -ErrorAction Continue
            Remove-Item $targetSpName -Force -ErrorAction Continue
        }
        catch {}
    }
    It "Rename-Item will rename a file" {
        Rename-Item $source $target

        Test-Path $source | Should -BeFalse
        Test-Path $target | Should -BeTrue

        Get-Content $target | should -Be "This is content"
    }
    It "Rename-Item will rename a file when path contains special char" {
        Rename-Item $sourceSp $targetSpName
        test-path $sourceSp | Should -BeFalse
        test-path -path $targetSp | Should -true

        Get-Content $targetSp | should -Be "This is content"
    }
#    It "Rename-Item will rename a file when -Path and CWD contains special char" {
#        $content = "This is content"
#        $oldSpName = "[orig]file.txt"
#        $oldSpBName = "``[orig``]file.txt"
#        $oldSp = "$wdSp/$oldSpBName"
#        $newSpName = "[renamed]file.txt"
#        $newSp = "$wdSp/``[renamed``]file.txt"
#        In $wdSp -Execute {
#            $null = New-Item -Name $oldSpName -ItemType File -Value $content -Force
#            Rename-Item -Path $oldSpBName $newSpName
#        }
#        $oldSp | Should -Not -Exist
#        $newSp | Should -Exist
#        $newSp | Should -FileContentMatchExactly $content
#    }
#    It "Rename-Item will rename a file when -LiteralPath and CWD contains special char" {
#        $content = "This is not content"
#        $oldSpName = "[orig]file2.txt"
#        $oldSpBName = "``[orig``]file2.txt"
#        $oldSp = "$wdSp/$oldSpBName"
#        $newSpName = "[renamed]file2.txt"
#        $newSp = "$wdSp/``[renamed``]file2.txt"
#        In $wdSp -Execute {
#            $null = New-Item -Name $oldSpName -ItemType File -Value $content -Force
#            Rename-Item -LiteralPath $oldSpName $newSpName
#        }
#        $oldSp | Should -Not -Exist
#        $newSp | Should -Exist
#        $newSp | Should -FileContentMatchExactly $content
#    }
}