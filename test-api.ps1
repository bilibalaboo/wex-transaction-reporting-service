$base = 'http://localhost:8080'
$pass = 0
$fail = 0

function Check($name, $got, $expected) {
    if ($got -eq $expected) {
        Write-Host "[PASS] $name (status $got)" -ForegroundColor Green
        $script:pass++
    } else {
        Write-Host "[FAIL] $name (got $got, expected $expected)" -ForegroundColor Red
        $script:fail++
    }
}

function Get-Status($url, $method = 'GET', $body = $null, $headers = @{}) {
    try {
        $params = @{ Uri = $url; Method = $method; UseBasicParsing = $true; TimeoutSec = 15; Headers = $headers }
        if ($body) { $params['Body'] = $body; $params['ContentType'] = 'application/json' }
        $r = Invoke-WebRequest @params -ErrorAction Stop
        return $r
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        return [PSCustomObject]@{ StatusCode = $code; Content = '' }
    }
}

Write-Host "`n=== WEX Transaction Reporting API Tests ===" -ForegroundColor Cyan

# 1. Create card
$r = Get-Status "$base/cards" POST '{"creditLimit":5000}'
$cardId = $r.Content.Trim('"')
Check '1. Create card' $r.StatusCode 201
Write-Host "   cardId: $cardId"

# 2. Invalid credit limit
$r = Get-Status "$base/cards" POST '{"creditLimit":-100}'
Check '2. Invalid credit limit' $r.StatusCode 422

# 3. Store transaction
$txBody = '{"description":"Hotel stay","transactionDate":"2025-10-01","amountUsd":250.00}'
$r = Get-Status "$base/cards/$cardId/transactions" POST $txBody @{'Idempotency-Key'='test-001'}
$txId = $r.Content.Trim('"')
Check '3. Store transaction' $r.StatusCode 201
Write-Host "   txId: $txId"

# 4. Idempotency replay
$r = Get-Status "$base/cards/$cardId/transactions" POST $txBody @{'Idempotency-Key'='test-001'}
$txId2 = $r.Content.Trim('"')
if ($r.StatusCode -eq 201 -and $txId -eq $txId2) {
    Write-Host "[PASS] 4. Idempotency replay — same txId returned" -ForegroundColor Green
    $script:pass++
} else {
    Write-Host "[FAIL] 4. Idempotency replay — status=$($r.StatusCode) sameId=$($txId -eq $txId2)" -ForegroundColor Red
    $script:fail++
}

# 5. Missing idempotency key
$r = Get-Status "$base/cards/$cardId/transactions" POST '{"description":"x","transactionDate":"2025-10-01","amountUsd":10}'
Check '5. Missing idempotency key' $r.StatusCode 400

# 6. Invalid amount
$r = Get-Status "$base/cards/$cardId/transactions" POST '{"description":"x","transactionDate":"2025-10-01","amountUsd":-50}' @{'Idempotency-Key'='test-002'}
Check '6. Invalid amount' $r.StatusCode 422

# 7. Card not found (store tx)
$r = Get-Status "$base/cards/00000000-0000-0000-0000-000000000000/transactions" POST '{"description":"x","transactionDate":"2025-10-01","amountUsd":10}' @{'Idempotency-Key'='test-003'}
Check '7. Card not found (store tx)' $r.StatusCode 404

# 8. Get transaction in EUR
$r = Get-Status "$base/transactions/$txId`?currency=Euro Zone-Euro"
Check '8. Get transaction in EUR' $r.StatusCode 200
if ($r.StatusCode -eq 200) {
    $j = $r.Content | ConvertFrom-Json
    Write-Host "   convertedAmount=$($j.convertedAmount)  rate=$($j.exchangeRateUsed)  rateDate=$($j.exchangeRateDate)"
}

# 9. Get transaction in CAD
$r = Get-Status "$base/transactions/$txId`?currency=Canada-Dollar"
Check '9. Get transaction in CAD' $r.StatusCode 200
if ($r.StatusCode -eq 200) {
    $j = $r.Content | ConvertFrom-Json
    Write-Host "   convertedAmount=$($j.convertedAmount)  rate=$($j.exchangeRateUsed)  rateDate=$($j.exchangeRateDate)"
}

# 10. Old transaction outside 6-month window
$r = Get-Status "$base/cards/$cardId/transactions" POST '{"description":"Pre-Euro purchase","transactionDate":"2001-01-01","amountUsd":100}' @{'Idempotency-Key'="window-test-$([guid]::NewGuid())"}
$oldTxId = $r.Content.Trim('"')
$r = Get-Status "$base/transactions/$oldTxId`?currency=Euro Zone-Euro"
Check '10. No rate in 6-month window (earliest EUR rate is 2001-03-31, tx date 2001-01-01)' $r.StatusCode 404

# 11. Transaction not found
$r = Get-Status "$base/transactions/00000000-0000-0000-0000-000000000000`?currency=Canada-Dollar"
Check '11. Transaction not found' $r.StatusCode 404

# 12. Card balance in EUR
$r = Get-Status "$base/cards/$cardId/balance`?currency=Euro Zone-Euro"
Check '12. Card balance in EUR' $r.StatusCode 200
if ($r.StatusCode -eq 200) {
    $j = $r.Content | ConvertFrom-Json
    Write-Host "   availableUsd=$($j.availableBalanceUsd)  converted=$($j.availableBalanceConverted)  rate=$($j.exchangeRateUsed)"
}

# 13. Card balance in CAD
$r = Get-Status "$base/cards/$cardId/balance`?currency=Canada-Dollar"
Check '13. Card balance in CAD' $r.StatusCode 200
if ($r.StatusCode -eq 200) {
    $j = $r.Content | ConvertFrom-Json
    Write-Host "   availableUsd=$($j.availableBalanceUsd)  converted=$($j.availableBalanceConverted)  rate=$($j.exchangeRateUsed)"
}

# 14. Card not found (balance)
$r = Get-Status "$base/cards/00000000-0000-0000-0000-000000000000/balance`?currency=Canada-Dollar"
Check '14. Card not found (balance)' $r.StatusCode 404

Write-Host "`n=== Results: $pass passed, $fail failed ===" -ForegroundColor Cyan
