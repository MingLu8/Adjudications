# pack.ps1

# 1. Clean old packages
Get-ChildItem "./bin/Debug/" -Filter "*.nupkg" -Recurse | Remove-Item -Force

# 2. Generate a unique version based on time (e.g., 1.0.0-pre.20260128)
$timestamp = Get-Date -Format "yyyyMMddHHmm"
$version = "1.0.0-pre.$timestamp"

Write-Host "Packing version $version..." -ForegroundColor Cyan

# 3. Pack the project
# This uses the <Content> tags we added to your .csproj earlier
dotnet pack --configuration Debug /p:PackageVersion=$version --output ./bin/Debug

# 4. Push to the local feed
$packageFile = Get-ChildItem -Path "$PSScriptRoot/bin/Debug/*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($packageFile) {
    Write-Host "Found package: $($packageFile.FullName)" -ForegroundColor Cyan
    # Push the specific file found
    dotnet nuget push $packageFile.FullName --source LocalFeed
} else {
    Write-Host "Error: Could not find any .nupkg file in $PSScriptRoot/bin/Debug/" -ForegroundColor Red
}

Write-Host "Success! Package version $version is now in LocalFeed." -ForegroundColor Green