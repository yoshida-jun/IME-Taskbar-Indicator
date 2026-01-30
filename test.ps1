# IME Color Indicator - ビルド・テストスクリプト

Write-Host "===== IME Color Indicator ビルド・テスト =====" -ForegroundColor Cyan

# エラー時に停止
$ErrorActionPreference = "Stop"

# プロジェクトパス
$projectPath = "$PSScriptRoot\IMEColorIndicator\IMEColorIndicator.csproj"

Write-Host "`n[1/4] 依存関係の復元中..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "依存関係の復元に失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/4] Debugビルド実行中..." -ForegroundColor Yellow
dotnet build $projectPath -c Debug --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Debugビルドに失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n[3/4] Releaseビルド実行中..." -ForegroundColor Yellow
dotnet build $projectPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Releaseビルドに失敗しました" -ForegroundColor Red
    exit 1
}

Write-Host "`n[4/4] 単体テスト実行中..." -ForegroundColor Yellow
# WPFアプリのため、実際の単体テストは現在なし
# 将来的にxUnitなどのテストプロジェクトを追加する場合はここでテストを実行
Write-Host "テストプロジェクトはまだ追加されていません" -ForegroundColor Yellow

Write-Host "`n===== ビルド・テスト完了 =====" -ForegroundColor Green
Write-Host "Debug:   .\IMEColorIndicator\bin\Debug\net10.0-windows\IMEColorIndicator.exe" -ForegroundColor Cyan
Write-Host "Release: .\IMEColorIndicator\bin\Release\net10.0-windows\IMEColorIndicator.exe" -ForegroundColor Cyan
