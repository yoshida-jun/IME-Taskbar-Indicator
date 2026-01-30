# IME Color Indicator - リリースビルドスクリプト

Write-Host "===== IME Color Indicator リリースビルド =====" -ForegroundColor Cyan

# エラー時に停止
$ErrorActionPreference = "Stop"

# プロジェクトパス
$projectPath = "$PSScriptRoot\IMEColorIndicator\IMEColorIndicator.csproj"
$outputPath = "$PSScriptRoot\publish"

Write-Host "`n[1/3] 依存関係の復元中..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "依存関係の復元に失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/3] Releaseビルド実行中..." -ForegroundColor Yellow
dotnet build $projectPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Releaseビルドに失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n[3/3] 単一実行ファイル生成中..." -ForegroundColor Yellow
dotnet publish $projectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false --no-build -o $outputPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "リリースビルドに失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n===== リリースビルド完了 =====" -ForegroundColor Green
Write-Host "出力先: $outputPath\IMEColorIndicator.exe" -ForegroundColor Cyan

# ファイルサイズを表示
$exePath = "$outputPath\IMEColorIndicator.exe"
if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host ("ファイルサイズ: {0:N2} MB" -f $fileSize) -ForegroundColor Cyan
}
