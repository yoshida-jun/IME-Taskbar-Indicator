# IME Color Indicator

IMEの状態（ON/OFF）を画面下端のカラーバーで視覚的に表示するWindowsアプリケーションです。

## 機能

- **IME状態の自動検出**: IMEのON/OFFを200msごとに監視
- **画面下端のカラーバー**: タスクバーの下に8pxの細いカラーバーを表示
- **カスタマイズ可能な色**:
  - カラーピッカーで自由に色を選択（1600万色以上）
  - 18色のプリセットカラーから選択可能
- **クリック透過**: カラーバーはクリックを透過し、タスクバーの操作を妨げません
- **設定の保存**: 色設定は自動的に保存され、次回起動時に復元されます
- **タスクトレイ常駐**: バックグラウンドで動作し、邪魔になりません
- **Windows自動起動**: 起動時に自動起動する設定が可能
- **自動アップデート**: GitHub Releasesから最新版を自動チェック・ダウンロード（1分間隔）

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
   - **Windows起動時に自動起動**: チェックボックスをONにすると次回から自動起動

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
- **自動アップデート**: アプリケーション起動中、1分間隔で最新版をチェックし、自動更新

## プロジェクト構成

```
IMEColorIndicator/
├── .github/
│   └── workflows/
│       ├── build.yml     # 自動ビルド・テストワークフロー
│       └── release.yml   # リリース自動化ワークフロー
├── IMEColorIndicator/
│   ├── App.xaml              # アプリケーションエントリポイント
│   ├── App.xaml.cs           # タスクトレイとIME監視の統合
│   ├── ColorBarWindow.xaml   # カラーバーウィンドウUI
│   ├── ColorBarWindow.xaml.cs # カラーバー表示ロジック
│   ├── SettingsWindow.xaml   # 設定画面UI
│   ├── SettingsWindow.xaml.cs # 設定画面ロジック
│   ├── ImeMonitor.cs         # IME状態監視
│   ├── Settings.cs           # 設定の保存/読み込み
│   ├── AutoStartHelper.cs    # Windows自動起動管理
│   └── Updater.cs            # 自動アップデート機能
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

## リリース方法

1. バージョン番号を更新: [IMEColorIndicator/IMEColorIndicator.csproj](IMEColorIndicator/IMEColorIndicator.csproj#L9)
2. 変更をコミット・プッシュ
3. タグを作成してプッシュ:
   ```bash
   git tag v0.1.0
   git push origin v0.1.0
   ```
4. GitHub Actionsが自動でビルド・リリースを実行
5. GitHub Releasesに実行ファイルとバージョンファイルがアップロードされる

## ライセンス

MIT License

## 開発者

作成日: 2026-01-29
