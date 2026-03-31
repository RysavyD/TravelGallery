$base = "C:/Dusan/Holy/TravelGallery/bin/Release/net10.0"
Add-Type -Path "$base/AngleSharp.dll"
Add-Type -Path "$base/AngleSharp.Css.dll"
Add-Type -Path "$base/HtmlSanitizer.dll"

$san = New-Object Ganss.Xss.HtmlSanitizer

$payloads = @(
    '<p>OK text</p>',
    '<script>alert("XSS")</script>',
    '<p>OK</p><script>alert("XSS")</script>',
    '<img src=x onerror="alert(1)">',
    '<iframe src="x"></iframe>',
    '<p>Bezpecny</p><script>alert("XSS777")</script><img src=x onerror="alert(1)"><iframe src="x"></iframe>'
)

foreach ($input in $payloads) {
    $output = $san.Sanitize($input)
    $changed = $input -ne $output
    Write-Host "INPUT:  $input"
    Write-Host "OUTPUT: $output"
    Write-Host "CHANGED: $changed"
    Write-Host "---"
}
