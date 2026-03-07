<#
PowerShell script to re-encode text files to UTF-8 (no BOM when possible).
Usage (from repo root):
  pwsh ./Tools/convert-encoding/convert-to-utf8.ps1
or (Windows PowerShell):
  .\Tools\convert-encoding\convert-to-utf8.ps1

It will process common text file extensions and rewrite them as UTF-8.
#>

$extensions = '*.cs','*.cshtml','*.css','*.js','*.json','*.md','*.txt','*.razor','*.xml'
$files = Get-ChildItem -Path . -Recurse -File -Include $extensions | Where-Object { $_.Length -gt 0 }
if ($files.Count -eq 0) {
    Write-Host "No files found to convert."
    exit 0
}

Write-Host "Found $($files.Count) files. Converting to UTF-8..."

foreach ($f in $files) {
    try {
        # Read with default encoding to recover characters; if it's already UTF8 this is safe
        $text = Get-Content -Raw -Encoding Default -Path $f.FullName
        # Try to use utf8NoBOM if available (PowerShell 6+), else use utf8 which may include BOM
        if ((Get-Command Set-Content).Parameters['Encoding'].ValidValues -contains 'utf8NoBOM') {
            Set-Content -Path $f.FullName -Value $text -Encoding utf8NoBOM
        }
        else {
            Set-Content -Path $f.FullName -Value $text -Encoding utf8
        }
        Write-Host "Converted: $($f.FullName)"
    }
    catch {
        Write-Warning "Failed: $($f.FullName) - $($_.Exception.Message)"
    }
}

Write-Host 'Conversion complete. Please reopen files in your editor and save if necessary.'
