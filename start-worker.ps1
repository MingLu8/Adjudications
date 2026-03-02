# 1. Define paths and port range
$appDir = Join-Path (Get-Location) "src\AdjudicationWorker\bin\Debug\net10.0"
$appName = "AdjudicationWorker.exe"
$startPort = 5050
$endPort = 5055

Write-Host "Launching AdjudicationWorker instances..." -ForegroundColor Cyan

# 2. Loop through the ports
for ($port = $startPort; $port -le $endPort; $port++) {
    Write-Host "Starting Instance on Port: $port" -ForegroundColor Green
    
    # Define environment variables for this specific process
    $envVars = @{
        "ASPNETCORE_URLS" = "http://localhost:$port"
    }

    # Start the process in a new window
    Start-Process -FilePath (Join-Path $appDir $appName) `
                  -WorkingDirectory $appDir `
                  -ArgumentList "--urls http://localhost:$port" `
                  -WindowStyle Normal
}

Write-Host "`nLaunching complete." -ForegroundColor Yellow
Pause