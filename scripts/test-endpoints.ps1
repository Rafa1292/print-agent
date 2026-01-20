# Test PrintAgent Endpoints
# Run this script to verify all endpoints are working

param(
    [string]$BaseUrl = "http://localhost:5123",
    [string]$PrinterName = "factura"
)

$ErrorActionPreference = "Continue"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  PrintAgent Endpoint Tests" -ForegroundColor Cyan
Write-Host "  URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null
    )

    Write-Host "Testing: $Name" -ForegroundColor Yellow
    Write-Host "  $Method $Url" -ForegroundColor Gray

    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
            TimeoutSec = 10
        }

        if ($Body) {
            $params.Body = $Body
        }

        $response = Invoke-RestMethod @params
        Write-Host "  [OK] " -ForegroundColor Green -NoNewline
        Write-Host ($response | ConvertTo-Json -Compress -Depth 3)
        return $true
    }
    catch {
        Write-Host "  [FAIL] " -ForegroundColor Red -NoNewline
        Write-Host $_.Exception.Message
        return $false
    }
}

$results = @()

# Test 1: Health
$results += Test-Endpoint -Name "Health Check" -Method "GET" -Url "$BaseUrl/health"
Write-Host ""

# Test 2: Printers
$results += Test-Endpoint -Name "Get Printers" -Method "GET" -Url "$BaseUrl/printers"
Write-Host ""

# Test 3: System Printers
$results += Test-Endpoint -Name "Get System Printers" -Method "GET" -Url "$BaseUrl/printers/system"
Write-Host ""

# Test 4: Config
$results += Test-Endpoint -Name "Get Config" -Method "GET" -Url "$BaseUrl/config"
Write-Host ""

# Test 5: Business Info
$results += Test-Endpoint -Name "Get Business Info" -Method "GET" -Url "$BaseUrl/business"
Write-Host ""

# Test 6: Print Test (solo si hay impresora configurada)
Write-Host "Testing: Print Test Page" -ForegroundColor Yellow
Write-Host "  POST $BaseUrl/print/test" -ForegroundColor Gray

$confirmation = Read-Host "  Enviar prueba de impresión a '$PrinterName'? (s/N)"
if ($confirmation -eq 's' -or $confirmation -eq 'S') {
    $body = @{ printerName = $PrinterName } | ConvertTo-Json
    $results += Test-Endpoint -Name "Print Test" -Method "POST" -Url "$BaseUrl/print/test" -Body $body
}
else {
    Write-Host "  [SKIP] Prueba de impresión omitida" -ForegroundColor Gray
}
Write-Host ""

# Test 7: Print Bill (mock data)
Write-Host "Testing: Print Bill (Mock)" -ForegroundColor Yellow
Write-Host "  POST $BaseUrl/print/bill" -ForegroundColor Gray

$confirmation = Read-Host "  Enviar factura de prueba a '$PrinterName'? (s/N)"
if ($confirmation -eq 's' -or $confirmation -eq 'S') {
    $billData = @{
        printerName = $PrinterName
        bill = @{
            id = "test-001"
            orderNumber = 1234
            deliveryMethod = "Mesa"
            tableNumber = "5"
            customerName = "Cliente de Prueba"
            subtotal = 100.00
            tax = 13.00
            discount = 0
            total = 113.00
            createdAt = (Get-Date).ToString("o")
            items = @(
                @{
                    id = "item-001"
                    description = "Producto de prueba 1"
                    quantity = 2
                    unitPrice = 25.00
                    subtotal = 50.00
                    discount = 0
                    tax = 6.50
                    total = 56.50
                    isCancelled = $false
                },
                @{
                    id = "item-002"
                    description = "Producto de prueba 2"
                    quantity = 1
                    unitPrice = 50.00
                    subtotal = 50.00
                    discount = 0
                    tax = 6.50
                    total = 56.50
                    isCancelled = $false
                }
            )
        }
    } | ConvertTo-Json -Depth 5

    $results += Test-Endpoint -Name "Print Bill" -Method "POST" -Url "$BaseUrl/print/bill" -Body $billData
}
else {
    Write-Host "  [SKIP] Impresión de factura omitida" -ForegroundColor Gray
}
Write-Host ""

# Summary
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Results Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

$passed = ($results | Where-Object { $_ -eq $true }).Count
$failed = ($results | Where-Object { $_ -eq $false }).Count
$total = $results.Count

Write-Host "  Passed: $passed / $total" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Yellow" })
if ($failed -gt 0) {
    Write-Host "  Failed: $failed" -ForegroundColor Red
}

Write-Host ""
if ($passed -eq $total) {
    Write-Host "All tests passed!" -ForegroundColor Green
}
else {
    Write-Host "Some tests failed. Check the output above." -ForegroundColor Yellow
}
