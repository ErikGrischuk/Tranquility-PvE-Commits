$rootPath = "C:\Users\Administrator\Desktop\ResFiles"

function Get-FileExtensionFromHeader {
    param ([byte[]]$bytes)

    $hex = ($bytes | ForEach-Object { $_.ToString("X2") }) -join ' '

    switch -Regex ($hex) {

        # ==== IMAGES ====
        '^89 50 4E 47'                   { return ".png" }
        '^FF D8 FF'                      { return ".jpg" }
        '^47 49 46 38'                   { return ".gif" }
        '^42 4D'                         { return ".bmp" }
        '^44 44 53 20'                   { return ".dds" }
        '^00 00 02 00'                   { return ".tga" }
        '^AB 4B 54 58'                   { return ".ktx" }
        '^AB 4B 54 58 20 32 30'          { return ".ktx2" }
        '^23 3F 52 41 44 49 41 4E 43 45' { return ".hdr" }
        '^49 49 2A 00|^4D 4D 00 2A'      { return ".tiff" }

        # ==== AUDIO ====
        '^4F 67 67 53'                   { return ".ogg" }
        '^66 4C 61 43'                   { return ".flac" }
        '^52 49 46 46 .* 57 45 4D'       { return ".wem" }
        '^42 4B 48 44'                   { return ".bnk" }
        '^49 44 33|^FF FB'               { return ".mp3" }
        '^52 49 46 46 .* 57 41 56 45'    { return ".wav" }

        # ==== VIDEO ====
        '^00 00 00 .* 66 74 79 70'       { return ".mp4" }
        '^1A 45 DF A3'                   { return ".mkv" }

        # ==== ARCHIVES ====
        '^50 4B 03 04'                   { return ".zip" }
        '^52 61 72 21'                   { return ".rar" }
        '^37 7A BC AF 27 1C'             { return ".7z" }
        '^75 73 74 61 72'               { return ".tar" }
        '^1F 8B'                         { return ".gz" }
        '^04 22 4D 18'                   { return ".lz4" }

        # ==== GAME / ENGINE ====
        '^55 4E 49 54 59 46 53'          { return ".unityfs" }
        '^55 4E 49 54 59'               { return ".asset" }
        '^42 55 4E 44 4C 45'            { return ".bundle" }
        '^50 41 4B'                      { return ".pak" }
        '^44 41 54'                      { return ".dat" }

        # ==== BINARIES ====
        '^4D 5A'                         { return ".exe" }
        '^7F 45 4C 46'                   { return ".elf" }
        '^CF FA ED FE|^CA FE BA BE'      { return ".macho" }

        default { return $null }
    }
}

Get-ChildItem -Path $rootPath -Recurse -File |
Where-Object { [System.IO.Path]::GetExtension($_.Name) -eq "" } |
ForEach-Object {

    try {
        $bytes = [byte[]]::new(32)
        $stream = [System.IO.File]::OpenRead($_.FullName)
        $null = $stream.Read($bytes, 0, 32)
        $stream.Close()

        $ext = Get-FileExtensionFromHeader $bytes

        if ($ext) {
            $newPath = "$($_.FullName)$ext"

            if (-not (Test-Path $newPath)) {
                Rename-Item $_.FullName $newPath
                Write-Host "Renamed: $($_.Name)$ext"
            }
        }
    }
    catch {
        Write-Host "Error: $($_.FullName)"
    }
}
