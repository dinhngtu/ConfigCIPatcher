Import-Module ConfigCI
Import-Module "$PSScriptRoot\ConfigCIPatcher.dll"
[ConfigCIPatcher.Patcher]::Patch()
