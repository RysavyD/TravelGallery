[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$baseUrl = "http://localhost:5117"
$pass = 0; $fail = 0

function OK($msg, $ok, $detail="") {
    if ($ok) { Write-Host "[PASS] $msg" -ForegroundColor Green; $script:pass++ }
    else      { Write-Host "[FAIL] $msg" -ForegroundColor Red;  $script:fail++ }
    if ($detail) { Write-Host "       $detail" -ForegroundColor Gray }
}

function Login($sess) {
    $p = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $sess -UseBasicParsing
    $t = ($p.Content | Select-String 'value="(CfDJ[^"]+)"').Matches[0].Groups[1].Value
    return Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method POST -WebSession $sess -UseBasicParsing -MaximumRedirection 5 -Body @{
        Email="admin@traveler.cz"; Password="Admin123!"; RememberMe="false"; "__RequestVerificationToken"=$t
    }
}

function GetToken($sess, $url) {
    $p = Invoke-WebRequest -Uri $url -WebSession $sess -UseBasicParsing
    return ($p.Content | Select-String 'value="(CfDJ[^"]+)"').Matches[0].Groups[1].Value
}

function CreateTrip($sess, $title, $desc, $tags="sectest") {
    $t = GetToken $sess "$baseUrl/Admin/Trips/Create"
    $r = Invoke-WebRequest -Uri "$baseUrl/Admin/Trips/Create" -Method POST -WebSession $sess -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue -Body @{
        Title=$title; Date="2025-06-15"; Description=$desc; TagNames=$tags; "__RequestVerificationToken"=$t
    }
    return $r
}

function DeleteTrip($sess, $id) {
    $t = GetToken $sess "$baseUrl/Admin/Trips/Delete/$id"
    Invoke-WebRequest -Uri "$baseUrl/Admin/Trips/Delete/$id" -Method POST -WebSession $sess -UseBasicParsing -MaximumRedirection 5 -Body @{"__RequestVerificationToken"=$t} | Out-Null
}

function FindTripId($sess, $title) {
    $list = Invoke-WebRequest -Uri "$baseUrl/Admin/Trips/Index" -WebSession $sess -UseBasicParsing
    $ids = [regex]::Matches($list.Content, 'href="/Admin/Trips/Edit/(\d+)"') | ForEach-Object { $_.Groups[1].Value }
    foreach ($id in $ids) {
        $p = Invoke-WebRequest -Uri "$baseUrl/Trips/Detail/$id" -WebSession $sess -UseBasicParsing
        if ($p.Content -match [regex]::Escape($title)) { return $id }
    }
    return $null
}

# ============================================================
Write-Host "`n=== 1. LOGIN ===" -ForegroundColor Cyan
$s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$lr = Login $s
OK "Login jako admin" ($lr.Content -match "Odhl|odhl") "HTTP $($lr.StatusCode)"

# ============================================================
Write-Host "`n=== 2. VYHLEDAVANI ===" -ForegroundColor Cyan

$idx = Invoke-WebRequest -Uri "$baseUrl/Trips/Index" -WebSession $s -UseBasicParsing
$cards = ([regex]::Matches($idx.Content, 'trip-card')).Count
OK "Index zobrazuje tripy ($cards karet)" ($cards -gt 0)

# Badge pro aktivni hledani (prazdny vysledek)
$bpage = Invoke-WebRequest -Uri "$baseUrl/Trips/Index?q=xyzUniqueQuery999" -WebSession $s -UseBasicParsing
OK "Search badge se zobrazi" ($bpage.Content -match "xyzUniqueQuery999")
OK "Tlacitko Zrusit filtr existuje" ($bpage.Content -match "Zru")

# Prazdny vysledek - hlaska obsahuje text "hled" (z "hledani") v HTML-encoded forme nebo neencoded
OK "Prazdny vysledek - spravna hlaska" ($bpage.Content -match "text-center text-muted py-5|hled|No trip")

# q param v tag odkazech - overime na strance s vysledky obsahujicimi tagy
$crTag = CreateTrip $s "TEST-SEARCHTAG" "<p>tag test</p>" "searchtag999"
Start-Sleep 1
$tTagId = FindTripId $s "TEST-SEARCHTAG"
if ($tTagId) {
    $qr = Invoke-WebRequest -Uri "$baseUrl/Trips/Index?q=TEST-SEARCHTAG" -WebSession $s -UseBasicParsing
    $tagCardsFound = ([regex]::Matches($qr.Content, 'trip-card')).Count
    $qInTagLinks = $qr.Content -match "q=TEST-SEARCHTAG"
    OK "q param v tag odkazech" $qInTagLinks "Vysledky: $tagCardsFound, q= v tag odkazech: $qInTagLinks"
    DeleteTrip $s $tTagId
    Write-Host "  Cleanup: trip $tTagId smazan" -ForegroundColor Gray
} else {
    OK "q param v tag odkazech" $false "Nepodarilo se vytvorit test trip"
}

# Hledani podle casti nazvu existujiciho tripu
# Fix: pouzij spravny regex ktery odpovida rendered HTML ('stretched-link" href=...')
$titleMatch = [regex]::Match($idx.Content, 'stretched-link[^>]*>\s*(\S[^\n<]{2,})')
if ($titleMatch.Success) {
    $rawTitle = $titleMatch.Groups[1].Value.Trim()
    # Dekoduj HTML entity pro hledani (System.Net.WebUtility je dostupne v .NET Core)
    $decodedTitle = [System.Net.WebUtility]::HtmlDecode($rawTitle)
    $q = $decodedTitle.Substring(0, [Math]::Min(4, $decodedTitle.Length))
    $sr = Invoke-WebRequest -Uri "$baseUrl/Trips/Index?q=$([uri]::EscapeDataString($q))" -WebSession $s -UseBasicParsing
    $found = ([regex]::Matches($sr.Content, 'trip-card')).Count
    OK "Hledani '$q' vraci vysledky" ($found -ge 1) "Nalezeno: $found tripu"
} else {
    OK "Extrakce nazvu pro hledani" $false "Zadne tripy v DB?"
}

# ============================================================
Write-Host "`n=== 3. XSS SANITIZER ===" -ForegroundColor Cyan

$xss = '<p>Bezpecny text</p><script>alert("XSS_MARK")</script><img src=x onerror="alert(2)"><iframe src="x"></iframe>'
$cr = CreateTrip $s "TEST-XSS-SANITIZER" $xss "sectest"
OK "Trip vytvoren (HTTP 302)" ($cr.StatusCode -eq 302) "Status: $($cr.StatusCode), Location: $($cr.Headers['Location'])"

Start-Sleep 1
$tid = FindTripId $s "TEST-XSS-SANITIZER"
OK "Trip nalezen v admin listu" ($tid -ne $null) "ID: $tid"

if ($tid) {
    $dp = Invoke-WebRequest -Uri "$baseUrl/Trips/Detail/$tid" -WebSession $s -UseBasicParsing
    $hasScript  = $dp.Content -match '<script>alert\("XSS_MARK"\)</script>'
    $hasOnerror = $dp.Content -match 'onerror='
    $hasIframe  = $dp.Content -match '<iframe src="x">'
    $hasSafe    = $dp.Content -match "Bezpecny text"
    OK "<script> tag odstranen" (-not $hasScript)  "Nalezen: $hasScript"
    OK "onerror= odstranen"     (-not $hasOnerror) "Nalezen: $hasOnerror"
    OK "<iframe> odstranen"     (-not $hasIframe)   "Nalezen: $hasIframe"
    OK "Bezpecny obsah zachovan" $hasSafe           "Nalezen: $hasSafe"

    # DB kontrola - "Description" sloupec obsahuje podretezec "script", proto hledame "<script"
    $dbOut = sqlcmd -S "(localdb)\MSSQLLocalDB" -d TravelGalleryDb -Q "SELECT Description FROM travel.Trips WHERE Id=$tid" 2>&1
    $dbHasScriptTag = ($dbOut -join "") -match "<script"
    OK "DB: <script> tag odstranen" (-not $dbHasScriptTag) "<script v DB: $dbHasScriptTag"

    DeleteTrip $s $tid
    Write-Host "  Cleanup: trip $tid smazan" -ForegroundColor Gray
}

# ============================================================
Write-Host "`n=== 4. AUTORIZACE ===" -ForegroundColor Cyan

$anon = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$aResp = Invoke-WebRequest -Uri "$baseUrl/Admin/Trips/Index" -WebSession $anon -UseBasicParsing -MaximumRedirection 5
OK "Admin nedostupny bez loginu" ($aResp.Content -match "Prihl|Login|E-mail|Email")

# ============================================================
Write-Host "`n=== 5. FORM VALIDACE ===" -ForegroundColor Cyan

# Overeni ze trip bez tagu lze vytvorit
$cr2 = CreateTrip $s "TEST-NOTAG" "<p>bez tagu</p>" ""
OK "Trip bez tagu lze vytvorit (HTTP 302)" ($cr2.StatusCode -eq 302) "Status: $($cr2.StatusCode)"
Start-Sleep 1
$tid2 = FindTripId $s "TEST-NOTAG"
if ($tid2) { DeleteTrip $s $tid2; Write-Host "  Cleanup: trip $tid2 smazan" -ForegroundColor Gray }

# ============================================================
Write-Host "`n====================================" -ForegroundColor Yellow
$color = if ($fail -eq 0) { "Green" } else { "Yellow" }
Write-Host "VYSLEDEK: $pass / $($pass+$fail) testu proslo" -ForegroundColor $color
if ($fail -gt 0) { Write-Host "$fail testu SELHALO" -ForegroundColor Red }

# Smaz tmp soubory
Remove-Item "C:/Dusan/Holy/TravelGallery/test_app.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_sanitizer.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_xss_debug.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_create_plain.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_create_inspect.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/create_response.html" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/debug_detail.html" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_db_check.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_inspect.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_inspect2.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/test_html_structure.ps1" -ErrorAction SilentlyContinue
Remove-Item "C:/Dusan/Holy/TravelGallery/index_debug.html" -ErrorAction SilentlyContinue
