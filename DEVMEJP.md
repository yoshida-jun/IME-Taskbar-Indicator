# IME Color Indicator - 開発者向けドキュメント

技術仕様、ビルド方法、リリース手順について説明します。

## プロジェクト構成

```
IMEColorIndicator/
├── .github/
│   └── workflows/
│       ├── build.yml     # 自動ビルド・テストワークフロー
│       └── release.yml   # リリース自動化ワークフロー
├── IMEColorIndicator/
│   ├── App.xaml.cs           # アプリケーションエントリポイント
│   ├── ColorBarWindow.xaml.cs # カラーバー表示ロジック
│   ├── SettingsWindow.xaml.cs # 設定画面ロジック
│   ├── ImeMonitor.cs         # IME状態監視
│   ├── Settings.cs           # 設定の保存/読み込み
│   ├── AutoStartHelper.cs    # Windows自動起動管理
│   ├── Updater.cs            # 自動アップデート機能
│   └── LocalizationHelper.cs # 多言語対応
├── test.ps1              # ビルド・テストスクリプト
├── build-release.ps1     # リリースビルドスクリプト
├── README.md             # 一般ユーザー向けドキュメント
├── DEVMEJP.md            # 開発者向けドキュメント（日本語）
└── DEVMEUS.md            # 開発者向けドキュメント（英語）
```

## 技術スタック

| 項目 | 技術 |
|------|------|
| **フレームワーク** | WPF (.NET 10.0) |
| **言語** | C# 13 |
| **IME検出** | Windows IMM32 API (`ImmGetContext`, `ImmGetOpenStatus`) |
| **クリック透過** | Win32 API (`WS_EX_TRANSPARENT`, `WS_EX_LAYERED`) |
| **設定保存** | JSON形式 (`System.Text.Json`) |
| **保存場所** | `%APPDATA%\IMEColorIndicator\settings.json` |
| **自動起動** | Windowsレジストリ (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) |
| **自動アップデート** | GitHub Releases API + バッチスクリプト |
| **多言語対応** | `GetUserDefaultUILanguage()` によるシステム言語検出 |

## ビルド方法

### 前提条件

- .NET 10.0 SDK
- Windows 10/11（WPF開発のため）

### 開発用ビルド

```bash
cd IMEColorIndicator
dotnet restore
dotnet build
```

実行ファイルは `bin\Debug\net10.0-windows\IMEColorIndicator.exe` に生成されます。

### リリースビルド（単一実行ファイル）

```powershell
# PowerShellでビルドスクリプトを実行
.\build-release.ps1
```

または手動で:

```powershell
dotnet publish IMEColorIndicator/IMEColorIndicator.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=false `
  -o ./publish
```

単一実行ファイルが `publish\IMEColorIndicator.exe` に生成されます。

**重要**: `PublishTrimmed=false` にしておかないと、リフレクション使用時に実行時エラーが発生します。

### テスト実行

```powershell
# ビルドとテストを実行
.\test.ps1
```

実際にビルドして実行し、動作確認してから次のステップに進んでください。

## CI/CD

GitHub Actionsによる自動ビルド・リリースに対応しています。

### 自動ビルド (.github/workflows/build.yml)

**トリガー**: master/mainブランチへのpushまたはPR

**処理内容**:
1. .NET 10.0 SDKのセットアップ
2. `dotnet restore`
3. `dotnet build -c Release`
4. `dotnet test`（テストプロジェクトがある場合）

### 自動リリース (.github/workflows/release.yml)

**トリガー**: `v*` タグのpush（例: `v0.3.1`）

**処理内容**:
1. .NET 10.0 SDKのセットアップ
2. `dotnet restore`
3. `dotnet build -c Release`
4. `dotnet publish`（single-file, self-contained）
5. `version.txt` の生成（タグ名から`v`を削除したバージョン番号）
6. GitHub Releasesへのアップロード:
   - `IMEColorIndicator.exe`
   - `version.txt`

## リリース手順

### 1. ローカルでビルド・テスト（必須）

```powershell
# ビルド
.\build-release.ps1

# 実行ファイルをテスト
.\publish\IMEColorIndicator.exe
```

**正常動作を確認してから次へ進む**

### 2. バージョン番号を更新

[IMEColorIndicator/IMEColorIndicator.csproj](IMEColorIndicator/IMEColorIndicator.csproj) の以下の行を編集:

```xml
<Version>0.3.1</Version>
<AssemblyVersion>0.3.1.0</AssemblyVersion>
<FileVersion>0.3.1.0</FileVersion>
```

### 3. 変更をコミット・プッシュ

```bash
git add -A
git commit -m "Bump version to 0.3.1"
git push origin master
```

### 4. タグを作成してプッシュ

```bash
git tag v0.3.1
git push origin v0.3.1
```

### 5. GitHub Actionsが自動実行

- GitHub Actionsが自動でビルド・リリースを実行
- 成功すると、GitHub Releasesに実行ファイルとバージョンファイルがアップロードされる
- リリースページ: `https://github.com/{OWNER}/{REPO}/releases/tag/v0.3.1`

### 6. リリースを確認

1. GitHub Releasesページで実行ファイルをダウンロード
2. 実行して正常動作を確認
3. 問題があればタグを削除してやり直し:
   ```bash
   git tag -d v0.3.1
   git push origin :refs/tags/v0.3.1
   ```

## 自動更新の仕組み

### チェック間隔

`Updater.cs` の `CHECK_INTERVAL` で定義（デフォルト: 1分）

本番環境では30分〜1時間に変更推奨:

```csharp
private static readonly TimeSpan CHECK_INTERVAL = TimeSpan.FromMinutes(30);
```

### 更新フロー

1. バックグラウンドで定期的に `version.txt` をチェック
2. リモートバージョン > ローカルバージョン の場合:
   - 新しい実行ファイルをダウンロード
   - 一時ファイル（`.new.exe`）として保存
   - アップデートスクリプト（`.update.bat`）を作成・実行
   - アプリを終了
3. バッチスクリプトが:
   - 2秒待機
   - 古い実行ファイルを削除
   - 新しい実行ファイルに置き換え
   - 新しい実行ファイルを起動
   - 自分自身（バッチスクリプト）を削除

### 注意事項

- single-file publish環境では `Process.GetCurrentProcess().MainModule?.FileName` が `null` を返すため、`Environment.ProcessPath` を優先的に使用
- Updaterのインスタンス化はtry-catchで保護し、更新機能が失敗してもアプリは起動できるようにする

## トラブルシューティング

### ビルドエラー: "Assets file doesn't have target for 'net10.0-windows/win-x64'"

**原因**: `--no-build` フラグが publish ステップで使用されている

**修正**: `dotnet publish` から `--no-build` を削除

### 実行時クラッシュ: すぐ終了する

**原因**: single-file publish環境で `Process.GetCurrentProcess().MainModule?.FileName` が `null`

**修正**: `Environment.ProcessPath` を優先的に使用（v0.3.1で修正済み）

### 自動更新が動作しない

**確認事項**:
1. 設定画面で「自動更新を有効にする」がONになっているか
2. GitHub Releasesに `version.txt` と `IMEColorIndicator.exe` が存在するか
3. バージョン番号が正しく記載されているか（`v` プレフィックスなし）
4. ファイアウォールでブロックされていないか

## ライセンス

MIT License

## 開発履歴

- **v0.3.1** (2026-01-30): single-file publish環境でのクラッシュ修正
- **v0.3.0** (2026-01-30): 言語切替、自動更新機能の追加
- **v0.2.0** (2026-01-29): 多言語対応、README二言語化
- **v0.1.0** (2026-01-29): 初回リリース、CI/CD構築
