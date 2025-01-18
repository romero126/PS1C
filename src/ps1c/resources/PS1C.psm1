if ($PSEdition -eq 'core') {
    Import-Module "$($PSScriptRoot)\Core\PS1C.dll"
} else {
    Import-Module "$($PSScriptRoot)\Desktop\PS1C.dll"
}