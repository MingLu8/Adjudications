# 1. Setup paths
$csprojPath = "./src/SharedContracts/SharedContracts.csproj"
$localFeed = "./LocalFeed" # Adjust path to your Katy local feed folder
# 2. Load and Increment Version
[xml]$xml = Get-Content $csprojPath
$versionNode = $xml.Project.PropertyGroup.Version
$version = [version]$versionNode
$newVersion = "{0}.{1}.{2}" -f $version.Major, $version.Minor, ($version.Build + 1)

Write-Host "--- Step 1: Bumping version to $newVersion ---" -ForegroundColor Cyan
$xml.Project.PropertyGroup.Version = $newVersion
$xml.Save($csprojPath)

# 3. Build the Project
# We build first to ensure Protos compile and Code Generation is successful
Write-Host "--- Step 2: Building Project ---" -ForegroundColor Cyan
dotnet build $csprojPath -c Release

if ($LASTEXITCODE -ne 0) { 
    Write-Host "Build failed! Aborting pack." -ForegroundColor Red
    exit $LASTEXITCODE 
}

# 4. Pack and Push
Write-Host "--- Step 3: Packing and Pushing ---" -ForegroundColor Cyan
dotnet pack $csprojPath -c Release --no-build --output $localFeed
dotnet nuget push "$localFeed/*.nupkg" --source $localFeed --skip-duplicate

Write-Host "DONE: Version $newVersion is now in $localFeed" -ForegroundColor Green
dotnet restore ./AdjudicationEngine.sln --force-evaluate