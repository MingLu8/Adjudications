# pack.ps1 (Now 2 levels up from the project)

# 1. Configuration - Set your project folder name here
$projectFolder = "SharedContracts" 
$projectPath = Join-Path $PSScriptRoot "src/$projectFolder" # Adjust "src" if needed
$outputPath = Join-Path $projectPath "bin/Debug"

# 2. Clean old packages
if (Test-Path $outputPath) {
    Get-ChildItem $outputPath -Filter "*.nupkg" -Recurse | Remove-Item -Force
}

# 3. Generate a unique version based on time
$timestamp = Get-Date -Format "yyyyMMddHHmm"
$version = "1.0.0-pre.$timestamp"

Write-Host "Packing version $version from $projectPath..." -ForegroundColor Cyan

# 4. Pack the project
# We run the command targeting the project folder specifically
dotnet pack $projectPath --configuration Debug /p:PackageVersion=$version --output $outputPath

# 5. Push to the local feed
$packageFile = Get-ChildItem -Path "$outputPath/*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($packageFile) {
    Write-Host "Found package: $($packageFile.FullName)" -ForegroundColor Cyan
    dotnet nuget push $packageFile.FullName --source LocalFeed
} else {
    Write-Host "Error: Could not find .nupkg in $outputPath" -ForegroundColor Red
}

Write-Host "Success! Package version $version is now in LocalFeed." -ForegroundColor Green