<table>
<tr>
<td width="50%" valign="top">

# IME Color Indicator

IMEの状態（ON/OFF）を画面端のカラーバーで視覚的に表示するWindowsアプリケーションです。

## ダウンロード

**[IMEColorIndicator.exe](https://github.com/yoshida-jun/IME-Taskbar-Indicator/releases/latest/download/IMEColorIndicator.exe)**

## 機能

- **IME状態の自動検出**: IMEのON/OFFを200msごとに監視
- **画面端のカラーバー**: 上下左右およびタスクバー上にカスタマイズ可能なカラーバーを表示
- **カスタマイズ可能な色**:
  - カラーピッカーで自由に色を選択（1600万色以上）
  - 18色のプリセットカラーから選択可能
- **クリック透過**: カラーバーはクリックを透過し、操作を妨げません
- **設定の保存**: 色設定は自動的に保存され、次回起動時に復元されます
- **タスクトレイ常駐**: バックグラウンドで動作し、邪魔になりません
- **Windows自動起動**: 起動時に自動起動する設定が可能
- **自動アップデート**: GitHub Releasesから最新版を自動チェック・ダウンロード（設定でON/OFF可能）

## 使い方

### 初回起動

1. `IMEColorIndicator.exe` を実行
2. 画面下端に青いカラーバー（IME OFF時のデフォルト色）が表示されます
3. IMEをONにすると、緑色（IME ON時のデフォルト色）に変わります

### 設定変更

1. タスクトレイの **[IME Color Indicator]** アイコンを右クリック
2. **「設定」** を選択
3. 設定画面で以下を変更できます：
   - **IME OFF時の色**: カラーピッカーまたはプリセットから選択
   - **IME ON時の色**: カラーピッカーまたはプリセットから選択
   - **バー表示設定**: 上下左右・タスクバー上の表示/非表示とサイズ
   - **Windows起動時に自動起動**: 次回から自動起動
   - **自動更新**: 最新版の自動チェック・ダウンロード（デフォルトOFF）

### 終了方法

1. タスクトレイの **[IME Color Indicator]** アイコンを右クリック
2. **「終了」** を選択

## システム要件

- Windows 10/11
- .NET 10.0 Runtime

## ビルド方法

### 開発用ビルド

```bash
cd IMEColorIndicator
dotnet build
```

実行ファイルは `bin\Debug\net10.0-windows\IMEColorIndicator.exe` に生成されます。

### リリースビルド（単一実行ファイル）

```powershell
# PowerShellでビルドスクリプトを実行
.\build-release.ps1
```

単一実行ファイルが `publish\IMEColorIndicator.exe` に生成されます。

### テスト実行

```powershell
# ビルドとテストを実行
.\test.ps1
```

## CI/CD

GitHub Actionsによる自動ビルド・リリースに対応しています。

- **自動ビルド**: master/mainブランチへのpushまたはPRで自動ビルド
- **自動リリース**: `v*` タグをpushすると自動でリリースビルドを実行し、GitHub Releasesにアップロード
- **自動アップデート**: 設定で有効にすると、1分間隔で最新版をチェックし、自動更新

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
└── build-release.ps1     # リリースビルドスクリプト
```

## 技術詳細

- **フレームワーク**: WPF (.NET 10.0)
- **IME検出**: Windows IMM32 API
- **クリック透過**: Win32 API (WS_EX_TRANSPARENT)
- **設定保存**: JSON形式 (`%APPDATA%\IMEColorIndicator\settings.json`)
- **自動起動**: Windowsレジストリ (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`)
- **自動アップデート**: GitHub Releases API + バッチスクリプトによる自動置換
- **多言語対応**: Windows システム言語検出（日本語/英語）

## リリース方法

1. バージョン番号を更新: [IMEColorIndicator/IMEColorIndicator.csproj](IMEColorIndicator/IMEColorIndicator.csproj#L10)
2. 変更をコミット・プッシュ
3. タグを作成してプッシュ:
   ```bash
   git tag v0.2.0
   git push origin v0.2.0
   ```
4. GitHub Actionsが自動でビルド・リリースを実行
5. GitHub Releasesに実行ファイルとバージョンファイルがアップロードされる

## ライセンス

MIT License

## 開発者

作成日: 2026-01-29

</td>
<td width="50%" valign="top">

# IME Color Indicator

A Windows application that visually displays IME status (ON/OFF) with color bars at screen edges.

## Download

**[IMEColorIndicator.exe](https://github.com/yoshida-jun/IME-Taskbar-Indicator/releases/latest/download/IMEColorIndicator.exe)**

## Features

- **Automatic IME Detection**: Monitors IME ON/OFF status every 200ms
- **Screen Edge Color Bars**: Customizable color bars at top, bottom, left, right, and above taskbar
- **Customizable Colors**:
  - Free color selection via color picker (16+ million colors)
  - 18 preset colors available
- **Click-Through**: Color bars don't interfere with operations
- **Settings Persistence**: Color settings are automatically saved and restored on next launch
- **System Tray Resident**: Runs in background without getting in the way
- **Windows Auto-Start**: Optional automatic launch at system startup
- **Auto-Update**: Automatic check and download of latest version from GitHub Releases (can be toggled ON/OFF)

## Usage

### First Launch

1. Run `IMEColorIndicator.exe`
2. A blue color bar (default IME OFF color) appears at the bottom of the screen
3. When IME is turned ON, it changes to green (default IME ON color)

### Changing Settings

1. Right-click the **[IME Color Indicator]** icon in the system tray
2. Select **"Settings"**
3. You can change the following in the settings window:
   - **IME OFF Color**: Select from color picker or presets
   - **IME ON Color**: Select from color picker or presets
   - **Bar Display Settings**: Show/hide and size for top, bottom, left, right, and taskbar bars
   - **Launch at Windows Startup**: Auto-start on next boot
   - **Auto Update**: Automatic check and download of latest version (default OFF)

### Exit

1. Right-click the **[IME Color Indicator]** icon in the system tray
2. Select **"Exit"**

## System Requirements

- Windows 10/11
- .NET 10.0 Runtime

## Build Instructions

### Development Build

```bash
cd IMEColorIndicator
dotnet build
```

Executable will be generated at `bin\Debug\net10.0-windows\IMEColorIndicator.exe`.

### Release Build (Single-File Executable)

```powershell
# Run build script in PowerShell
.\build-release.ps1
```

Single-file executable will be generated at `publish\IMEColorIndicator.exe`.

### Test Execution

```powershell
# Run build and tests
.\test.ps1
```

## CI/CD

Automated build and release with GitHub Actions.

- **Automated Build**: Automatic build on push or PR to master/main branch
- **Automated Release**: Push a `v*` tag to automatically build and upload to GitHub Releases
- **Auto-Update**: When enabled in settings, checks for updates every 1 minute and auto-updates

## Project Structure

```
IMEColorIndicator/
├── .github/
│   └── workflows/
│       ├── build.yml     # Automated build and test workflow
│       └── release.yml   # Release automation workflow
├── IMEColorIndicator/
│   ├── App.xaml.cs           # Application entry point
│   ├── ColorBarWindow.xaml.cs # Color bar display logic
│   ├── SettingsWindow.xaml.cs # Settings window logic
│   ├── ImeMonitor.cs         # IME state monitoring
│   ├── Settings.cs           # Settings save/load
│   ├── AutoStartHelper.cs    # Windows auto-start management
│   ├── Updater.cs            # Auto-update functionality
│   └── LocalizationHelper.cs # Localization support
├── test.ps1              # Build and test script
└── build-release.ps1     # Release build script
```

## Technical Details

- **Framework**: WPF (.NET 10.0)
- **IME Detection**: Windows IMM32 API
- **Click-Through**: Win32 API (WS_EX_TRANSPARENT)
- **Settings Storage**: JSON format (`%APPDATA%\IMEColorIndicator\settings.json`)
- **Auto-Start**: Windows Registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`)
- **Auto-Update**: GitHub Releases API + batch script for automatic replacement
- **Localization**: Windows system language detection (Japanese/English)

## Release Process

1. Update version number: [IMEColorIndicator/IMEColorIndicator.csproj](IMEColorIndicator/IMEColorIndicator.csproj#L10)
2. Commit and push changes
3. Create and push tag:
   ```bash
   git tag v0.2.0
   git push origin v0.2.0
   ```
4. GitHub Actions automatically builds and releases
5. Executable and version file are uploaded to GitHub Releases

## License

MIT License

## Developer

Created: 2026-01-29

</td>
</tr>
</table>
