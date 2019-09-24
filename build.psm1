function Install-Dotnet {
    [CmdletBinding()]
    param(
        [string]$Channel = $dotnetCLIChannel,
        [string]$Version = $dotnetCLIRequiredVersion,
        [switch]$NoSudo
    )

    # This allows sudo install to be optional; needed when running in containers / as root
    # Note that when it is null, Invoke-Expression (but not &) must be used to interpolate properly
    $sudo = if (!$NoSudo) { "sudo" }

    $installObtainUrl = "https://dot.net/v1"
    $uninstallObtainUrl = "https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain"

    # Install for Linux and OS X
    if ($Environment.IsLinux -or $Environment.IsMacOS) {
        # Uninstall all previous dotnet packages
        $uninstallScript = if ($Environment.IsUbuntu) {
            "dotnet-uninstall-debian-packages.sh"
        } elseif ($Environment.IsMacOS) {
            "dotnet-uninstall-pkgs.sh"
        }

        if ($uninstallScript) {
            Start-NativeExecution {
                curl -sO $uninstallObtainUrl/uninstall/$uninstallScript
                Invoke-Expression "$sudo bash ./$uninstallScript"
            }
        } else {
            Write-Warning "This script only removes prior versions of dotnet for Ubuntu and OS X"
        }

        # Install new dotnet 1.1.0 preview packages
        $installScript = "dotnet-install.sh"
        Start-NativeExecution {
            curl -sO $installObtainUrl/$installScript
            bash ./$installScript -c $Channel -v $Version
        }
    } elseif ($Environment.IsWindows) {
        Remove-Item -ErrorAction SilentlyContinue -Recurse -Force ~\AppData\Local\Microsoft\dotnet
        $installScript = "dotnet-install.ps1"
        Invoke-WebRequest -Uri $installObtainUrl/$installScript -OutFile $installScript

        if (-not $Environment.IsCoreCLR) {
            & ./$installScript -Channel $Channel -Version $Version
        } else {
            # dotnet-install.ps1 uses APIs that are not supported in .NET Core, so we run it with Windows PowerShell
            $fullPSPath = Join-Path -Path $env:windir -ChildPath "System32\WindowsPowerShell\v1.0\powershell.exe"
            $fullDotnetInstallPath = Join-Path -Path $pwd.Path -ChildPath $installScript
            Start-NativeExecution { & $fullPSPath -NoLogo -NoProfile -File $fullDotnetInstallPath -Channel $Channel -Version $Version }
        }
    }
}


function Start-ResGen
{
    [CmdletBinding()]
    param()

    # Add .NET CLI tools to PATH
    Find-Dotnet

    Push-Location "$PSScriptRoot/Source/ResGen"

    try {
        Start-NativeExecution { dotnet run } | Write-Verbose
    } finally {
        Pop-Location
    }
}

function Find-Dotnet() {
    $originalPath = $env:PATH
    $dotnetPath = if ($Environment.IsWindows) { "$env:LocalAppData\Microsoft\dotnet" } else { "$env:HOME/.dotnet" }

    # If there dotnet is already in the PATH, check to see if that version of dotnet can find the required SDK
    # This is "typically" the globally installed dotnet
    if (precheck dotnet) {
        # Must run from within repo to ensure global.json can specify the required SDK version
        Push-Location $PSScriptRoot
        $dotnetCLIInstalledVersion = (dotnet --version)
        Pop-Location
        if ($dotnetCLIInstalledVersion -ne $dotnetCLIRequiredVersion) {
            Write-Warning "The 'dotnet' in the current path can't find SDK version ${dotnetCLIRequiredVersion}, prepending $dotnetPath to PATH."
            # Globally installed dotnet doesn't have the required SDK version, prepend the user local dotnet location
            $env:PATH = $dotnetPath + [IO.Path]::PathSeparator + $env:PATH
        }
    }
    else {
        Write-Warning "Could not find 'dotnet', appending $dotnetPath to PATH."
        $env:PATH += [IO.Path]::PathSeparator + $dotnetPath
    }

    if (-not (precheck 'dotnet' "Still could not find 'dotnet', restoring PATH.")) {
        $env:PATH = $originalPath
    }
}

function script:Use-MSBuild {
    # TODO: we probably should require a particular version of msbuild, if we are taking this dependency
    # msbuild v14 and msbuild v4 behaviors are different for XAML generation
    $frameworkMsBuildLocation = "${env:SystemRoot}\Microsoft.Net\Framework\v4.0.30319\msbuild"

    $msbuild = get-command msbuild -ErrorAction Ignore
    if ($msbuild) {
        # all good, nothing to do
        return
    }

    if (-not (Test-Path $frameworkMsBuildLocation)) {
        throw "msbuild not found in '$frameworkMsBuildLocation'. Install Visual Studio 2015."
    }

    Set-Alias msbuild $frameworkMsBuildLocation -Scope Script
}

function script:Write-Log
{
    param
    (
        [Parameter(Position=0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $message,

        [switch] $error
    )
    if ($error)
    {
        Write-Host -Foreground Red $message
    }
    else
    {
        Write-Host -Foreground Green $message
    }
    #reset colors for older package to at return to default after error message on a compilation error
    [console]::ResetColor()
}
function script:precheck([string]$command, [string]$missedMessage) {
    $c = Get-Command $command -ErrorAction Ignore
    if (-not $c) {
        if (-not [string]::IsNullOrEmpty($missedMessage))
        {
            Write-Warning $missedMessage
        }
        return $false
    } else {
        return $true
    }
}

# this function wraps native command Execution
# for more information, read https://mnaoumov.wordpress.com/2015/01/11/execution-of-external-commands-in-powershell-done-right/
function script:Start-NativeExecution
{
    param(
        [scriptblock]$sb,
        [switch]$IgnoreExitcode,
        [switch]$VerboseOutputOnError
    )
    $backupEAP = $script:ErrorActionPreference
    $script:ErrorActionPreference = "Continue"
    try {
        if($VerboseOutputOnError.IsPresent)
        {
            $output = & $sb 2>&1
        }
        else
        {
            & $sb
        }

        # note, if $sb doesn't have a native invocation, $LASTEXITCODE will
        # point to the obsolete value
        if ($LASTEXITCODE -ne 0 -and -not $IgnoreExitcode) {
            if($VerboseOutputOnError.IsPresent -and $output)
            {
                $output | Out-String | Write-Verbose -Verbose
            }

            # Get caller location for easier debugging
            $caller = Get-PSCallStack -ErrorAction SilentlyContinue
            if($caller)
            {
                $callerLocationParts = $caller[1].Location -split ":\s*line\s*"
                $callerFile = $callerLocationParts[0]
                $callerLine = $callerLocationParts[1]

                $errorMessage = "Execution of {$sb} by ${callerFile}: line $callerLine failed with exit code $LASTEXITCODE"
                throw $errorMessage
            }
            throw "Execution of {$sb} failed with exit code $LASTEXITCODE"
        }
    } finally {
        $script:ErrorActionPreference = $backupEAP
    }
}