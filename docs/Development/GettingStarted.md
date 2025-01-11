# Prerequisites
* VSCode (Is the preferred method)
* DotNet SDK 8.0
* Nuget Repository Configured
* Install Module Pester
``` ps1
Install-Module Pester -Force
```

# Build the code

Running `dotnet build` from the root of the project will build the project

``` ps1
# Build the code
dotnet build
```

# Pester Test the code

``` ps1
. .\Tests\Test.ps1
```

# Custom Tests and Debugging

``` ps1

```