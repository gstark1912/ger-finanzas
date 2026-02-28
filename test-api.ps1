# API Test Script
# Run this after starting the API to verify endpoints

Write-Host "Testing Finanzas API..." -ForegroundColor Green
Write-Host ""

$baseUrl = "http://localhost:5080/api/expense-accounts"

# Test 1: Get all accounts
Write-Host "1. GET all accounts..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method Get
    Write-Host "   ✓ Success! Found $($response.Count) accounts" -ForegroundColor Green
    $response | Format-Table -Property name, type, currency, isActive
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 2: Create new account
Write-Host "2. POST new account..." -ForegroundColor Yellow
$newAccount = @{
    name = "Test Account"
    type = "Bank"
    currency = "USD"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $newAccount -ContentType "application/json"
    Write-Host "   ✓ Success! Created account: $($response.name)" -ForegroundColor Green
    $testId = $response.id
} catch {
    Write-Host "   ✗ Failed: $_" -ForegroundColor Red
}

Write-Host ""

# Test 3: Get single account
if ($testId) {
    Write-Host "3. GET single account..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$testId" -Method Get
        Write-Host "   ✓ Success! Retrieved: $($response.name)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Failed: $_" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # Test 4: Update account
    Write-Host "4. PUT update account..." -ForegroundColor Yellow
    $updateAccount = @{
        name = "Updated Test Account"
        type = "Cash"
        currency = "ARS"
        isActive = $true
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/$testId" -Method Put -Body $updateAccount -ContentType "application/json"
        Write-Host "   ✓ Success! Updated to: $($response.name)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Failed: $_" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # Test 5: Delete account
    Write-Host "5. DELETE account..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/$testId" -Method Delete
        Write-Host "   ✓ Success! Account soft-deleted" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Failed: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "API tests completed!" -ForegroundColor Green
