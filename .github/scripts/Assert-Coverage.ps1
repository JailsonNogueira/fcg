[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $CoverageFile,

    [Parameter(Mandatory)]
    [string] $Assembly,

    [Parameter(Mandatory)]
    [ValidateRange(0, 100)]
    [double] $Minimum
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CoverageFile -PathType Leaf)) {
    throw "Coverage report '$CoverageFile' was not generated."
}

[xml] $report = Get-Content -LiteralPath $CoverageFile -Raw
$packages = @($report.coverage.packages.package) | Where-Object { $_ -and $_.name }
$assemblyPackage = @($packages | Where-Object {
    $_.name -eq $Assembly -or $_.name.StartsWith("$Assembly.")
})

if (-not $assemblyPackage) {
    throw "Assembly '$Assembly' was not found in the coverage report. Check that the test projects reference it and that its tests ran."
}

$package = $assemblyPackage | Select-Object -First 1
$lineRateText = [string] $package.'line-rate'
if ([string]::IsNullOrWhiteSpace($lineRateText)) {
    throw "Assembly '$Assembly' in '$CoverageFile' does not contain a line-rate value."
}

$lineRate = 0.0
if (-not [double]::TryParse(
    $lineRateText,
    [Globalization.NumberStyles]::Float,
    [Globalization.CultureInfo]::InvariantCulture,
    [ref] $lineRate
)) {
    throw "Invalid line-rate value '$lineRateText' in '$CoverageFile'."
}

$percentage = $lineRate * 100
Write-Host ("{0} line coverage: {1:N2}% (required: {2:N2}%)" -f $Assembly, $percentage, $Minimum)

if ($percentage + 0.000001 -lt $Minimum) {
    throw ("Coverage gate failed: {0:N2}% is below the required {1:N2}%." -f $percentage, $Minimum)
}

Write-Host 'Coverage gate passed.'
