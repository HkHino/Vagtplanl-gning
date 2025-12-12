param(
    # Sti til test-projektet
    [string]$TestProjectPath = ".\Vagtplanlægning.UnitTests\Vagtplanlægning.UnitTests.csproj",

    # Hvor coverage-filen skal ligge (uden .cobertura.xml suffix – det tilføjes automatisk)
    [string]$CoverletOutputPrefix = ".\Vagtplanlægning.UnitTests\TestResults\coverage",

    # Mappe til HTML-coverage-rapporten
    [string]$ReportDir = ".\coveragereport"
)

Write-Host "=== STEP 1: Kører dotnet test med coverage ==="

dotnet test $TestProjectPath `
  /p:CollectCoverage=true `
  /p:CoverletOutput=$CoverletOutputPrefix `
  /p:CoverletOutputFormat=cobertura

if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet test fejlede – coverage blev ikke genereret." -ForegroundColor Red
    exit 1
}

# Forventet sti til coverage.cobertura.xml
$coveragePath = "$CoverletOutputPrefix.cobertura.xml"

Write-Host ""
Write-Host "=== STEP 2: Finder coverage.cobertura.xml ==="

# Søg i hele testprojektet efter coverage.cobertura.xml
$coverageFiles = Get-ChildItem -Path ".\Vagtplanlægning.UnitTests" -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue

if (-not $coverageFiles) {
    Write-Host "Kunne ikke finde nogen coverage.cobertura.xml under '.\Vagtplanlægning.UnitTests'." -ForegroundColor Red
    Write-Host "Tjek at coverlet.msbuild er installeret på testprojektet, fx:"
    Write-Host "  dotnet add .\Vagtplanlægning.UnitTests\Vagtplanlægning.UnitTests.csproj package coverlet.msbuild"
    exit 1
}

# Tag den nyeste coverage-fil (hvis der er flere)
$coverageFile = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$coveragePath = $coverageFile.FullName

Write-Host "Bruger coverage-fil: $coveragePath"



Write-Host ""
Write-Host "=== STEP 3: Genererer HTML-rapport med reportgenerator ==="

if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "reportgenerator blev ikke fundet i PATH." -ForegroundColor Red
    Write-Host "Installer den med:"
    Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool"
    exit 1
}

reportgenerator `
  -reports:"$coveragePath" `
  -targetdir:"$ReportDir" `
  -reporttypes:Html

if ($LASTEXITCODE -ne 0) {
    Write-Host "reportgenerator fejlede." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== STEP 4: Åbner rapporten i browseren ==="

$indexPath = Join-Path $ReportDir "index.html"

if (Test-Path $indexPath) {
    Write-Host "Åbner rapport: $indexPath"
    Start-Process $indexPath
} else {
    Write-Host "Kunne ikke finde index.html i '$ReportDir'." -ForegroundColor Yellow
}
