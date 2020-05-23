$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$zipFilePath = Join-Path $toolsDir 'codecov-win7-x64.zip'
Get-ChocolateyUnzip -FileFullPath $zipFilePath -Destination $toolsDir
