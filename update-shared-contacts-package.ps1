# update-consumers.ps1

# 1. Configuration
$packageName = "SharedContracts"
$localFeedPath = "D:\_dev\LocalNuGet"
$searchRoot = $PSScriptRoot

# Delete the local cache for this specific package
Remove-Item -Recurse -Force "$env:USERPROFILE\.nuget\packages\sharedcontracts"

# Clear the global NuGet HTTP cache just in case
# dotnet nuget locals all --clear

# 2. Find the latest version from the local feed
Write-Host "Searching for latest version of $packageName in $localFeedPath..." -ForegroundColor Cyan

$latestPackage = Get-ChildItem -Path $localFeedPath -Filter "$packageName.*.nupkg" | 
                 Sort-Object LastWriteTime -Descending | 
                 Select-Object -First 1

if (-not $latestPackage) {
    Write-Error "Could not find any packages for $packageName in $localFeedPath"
    exit
}

# Extract version from filename (e.g., SharedContracts.1.0.0-pre.nupkg -> 1.0.0-pre)
$latestVersion = $latestPackage.Name -replace "^$packageName\.", "" -replace "\.nupkg$", ""
Write-Host "Latest version found: $latestVersion" -ForegroundColor Green

# 3. Find all .csproj files that reference this package
$projects = Get-ChildItem -Path $searchRoot -Filter "*.csproj" -Recurse

foreach ($project in $projects) {
    [xml]$xml = Get-Content $project.FullName
    
    # Find the PackageReference node for SharedContracts
    $node = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq $packageName }
    
    if ($node) {
        if ($node.Version -ne $latestVersion) {
            Write-Host "Updating $($project.Name): $($node.Version) -> $latestVersion" -ForegroundColor Yellow
            $node.Version = $latestVersion
            $xml.Save($project.FullName)
        } else {
            Write-Host "$($project.Name) is already up to date." -ForegroundColor Gray
        }
    }
}

# 4. Global Restore to ensure paths $(PkgSharedContracts) refresh
Write-Host "Running dotnet restore..." -ForegroundColor Cyan
dotnet restore $searchRoot