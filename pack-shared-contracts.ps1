# 1. Setup paths
$csprojPath = "./src/SharedContracts/SharedContracts.csproj"
$localFeed = "./LocalFeed" 

# 2. Load and Increment Version
[xml]$xml = Get-Content $csprojPath
$versionNode = $xml.Project.PropertyGroup.Version
$version = [version]$versionNode
$newVersion = "{0}.{1}.{2}" -f $version.Major, $version.Minor, ($version.Build + 1)

Write-Host "--- Step 1: Bumping version to $newVersion ---" -ForegroundColor Cyan
$xml.Project.PropertyGroup.Version = $newVersion
$xml.Save($csprojPath)

# 3. Build the Project (Changed to Debug)
Write-Host "--- Step 2: Building Project (Debug) ---" -ForegroundColor Cyan
dotnet build $csprojPath -c Debug

if ($LASTEXITCODE -ne 0) { 
    Write-Host "Build failed! Aborting pack." -ForegroundColor Red
    exit $LASTEXITCODE 
}

# 4. Pack and Push (Changed to Debug)
Write-Host "--- Step 3: Packing and Pushing ---" -ForegroundColor Cyan
dotnet pack $csprojPath -c Debug --no-build --output $localFeed
dotnet nuget push "$localFeed/*.nupkg" --source $localFeed --skip-duplicate

Write-Host "DONE: Version $newVersion is now in $localFeed" -ForegroundColor Green
dotnet restore ./AdjudicationEngine.slnx --force-evaluate