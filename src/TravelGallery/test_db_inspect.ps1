$s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$p = Invoke-WebRequest -Uri 'http://localhost:5117/Account/Login' -WebSession $s -UseBasicParsing
$t = ($p.Content | Select-String 'value="(CfDJ[^"]+)"').Matches[0].Groups[1].Value
Invoke-WebRequest -Uri 'http://localhost:5117/Account/Login' -Method POST -Body @{
    Email="admin@traveler.cz"; Password="Admin123!"; RememberMe="false"; "__RequestVerificationToken"=$t
} -WebSession $s -UseBasicParsing -MaximumRedirection 5 | Out-Null
Write-Host "Prihlasen"

$cp = Invoke-WebRequest -Uri 'http://localhost:5117/Admin/Trips/Create' -WebSession $s -UseBasicParsing
$ct = ($cp.Content | Select-String 'value="(CfDJ[^"]+)"').Matches[0].Groups[1].Value

$xss = '<p>SanitTest</p><script>alert("XSS777")</script><img src=x onerror="alert(1)">'
Write-Host "Input: $xss"

$r = Invoke-WebRequest -Uri 'http://localhost:5117/Admin/Trips/Create' -Method POST -Body @{
    Title="SAN-TEST-777"; Date="2025-06-15"; Description=$xss; TagNames="sec"
    "__RequestVerificationToken"=$ct
} -WebSession $s -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
Write-Host "Create: HTTP $($r.StatusCode) -> $($r.Headers['Location'])"

Start-Sleep 1
# Zjisti ID
$list = Invoke-WebRequest -Uri 'http://localhost:5117/Admin/Trips/Index' -WebSession $s -UseBasicParsing
$ids = [regex]::Matches($list.Content, 'href="/Admin/Trips/Edit/(\d+)"') | ForEach-Object { $_.Groups[1].Value }
$tid = $null
foreach ($id in $ids) {
    $dp = Invoke-WebRequest -Uri "http://localhost:5117/Trips/Detail/$id" -WebSession $s -UseBasicParsing
    if ($dp.Content -match "SAN-TEST-777") { $tid = $id; break }
}
Write-Host "Trip ID: $tid"

if ($tid) {
    # Ziskej presny Description z DB
    Write-Host "`n--- RAW DB DESCRIPTION ---"
    $q = "SELECT Description FROM travel.Trips WHERE Id=$tid"
    $dbOut = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TravelGalleryDb -Q $q 2>&1
    Write-Host ($dbOut -join "`n")

    # Cleanup
    $dp2 = Invoke-WebRequest -Uri "http://localhost:5117/Admin/Trips/Delete/$tid" -WebSession $s -UseBasicParsing
    $dt = ($dp2.Content | Select-String 'value="(CfDJ[^"]+)"').Matches[0].Groups[1].Value
    Invoke-WebRequest -Uri "http://localhost:5117/Admin/Trips/Delete/$tid" -Method POST -Body @{"__RequestVerificationToken"=$dt} -WebSession $s -UseBasicParsing -MaximumRedirection 5 | Out-Null
    Write-Host "Smazano"
}
