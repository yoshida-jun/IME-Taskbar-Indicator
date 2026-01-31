<table>
<tr>
<td width="50%" valign="top">

# IME Color Indicator

IMEの状態（ON/OFF）を画面端のカラーバーで視覚的に表示するWindowsアプリケーションです。

## ダウンロード

**[最新版をダウンロード](https://github.com/yoshida-jun/IME-Taskbar-Indicator/releases/latest/download/IMEColorIndicator.exe)**

ダウンロードしたファイルを実行するだけで使用できます。インストール不要です。

## 主な機能

- **IME状態を色で表示**: IMEのON/OFFを画面端のカラーバーで表示
- **自由にカスタマイズ**: 色とバーの位置・サイズを自由に設定
- **邪魔にならない**: カラーバーはクリックを透過し、操作を妨げません
- **解像度変更に対応**: ディスプレイの解像度変更時に自動で再配置
- **自動起動対応**: Windows起動時に自動起動できます
- **多言語対応**: 日本語・英語に対応（自動検出または手動切替）
- **自動更新**: 最新版を自動でチェック・ダウンロード（設定でON/OFF可能）

## 使い方

### 初回起動

1. ダウンロードした `IMEColorIndicator.exe` をダブルクリック
2. 画面下端に青いバー（IME OFF時）が表示されます
3. IMEをONにすると緑色（IME ON時）に変わります

### 設定変更

1. タスクトレイのアイコンを**左クリック**または**右クリック → 設定**
2. 設定画面で以下を変更できます：
   - **色の設定**: IME ON/OFF時の色を選択
   - **バーの表示**: 上下左右・タスクバー上の表示/非表示とサイズ
   - **言語**: 自動検出 / 日本語 / English
   - **自動起動**: Windows起動時に自動起動
   - **自動更新**: 最新版の自動チェック（デフォルトOFF）

### 終了方法

タスクトレイのアイコンを右クリック → **「終了」**

## システム要件

- **OS**: Windows 10 / Windows 11
- **.NET**: 9.0 Runtime（含まれています、別途インストール不要）

## よくある質問

**Q: インストールは必要ですか？**
A: 不要です。ダウンロードした実行ファイルをダブルクリックするだけで使えます。

**Q: アンインストール方法は？**
A: アプリを終了して、実行ファイルを削除するだけです。設定ファイルは `%APPDATA%\IMEColorIndicator` にあります。

**Q: 自動更新は安全ですか？**
A: デフォルトで無効です。有効にすると、GitHub Releasesから公式の最新版のみをダウンロードします。

**Q: カラーバーが表示されません**
A: 設定画面でバーの表示がONになっているか、サイズが0pxになっていないか確認してください。

## ライセンス

MIT License

## 開発者向け情報

技術仕様やビルド方法については [DEVMEJP.md](DEVMEJP.md) をご覧ください。

---

作成日: 2026-01-29

</td>
<td width="50%" valign="top">

# IME Color Indicator

A Windows application that visually displays IME status (ON/OFF) with color bars at screen edges.

## Download

**[Download Latest Version](https://github.com/yoshida-jun/IME-Taskbar-Indicator/releases/latest/download/IMEColorIndicator.exe)**

Just run the downloaded file. No installation required.

## Key Features

- **Visual IME Status**: Display IME ON/OFF with color bars at screen edges
- **Fully Customizable**: Freely configure colors and bar position/size
- **Non-Intrusive**: Color bars are click-through and don't interfere
- **Resolution Change Support**: Automatically repositions on display resolution changes
- **Auto-Start Support**: Can launch automatically at Windows startup
- **Multi-Language**: Japanese/English support (auto-detect or manual switch)
- **Auto-Update**: Automatic check and download of latest version (toggleable)

## How to Use

### First Launch

1. Double-click the downloaded `IMEColorIndicator.exe`
2. A blue bar (IME OFF) appears at the bottom of the screen
3. When IME is ON, it changes to green

### Settings

1. **Left-click** the tray icon or **right-click → Settings**
2. You can configure:
   - **Colors**: Choose colors for IME ON/OFF
   - **Bar Display**: Show/hide and size for top/bottom/left/right/taskbar bars
   - **Language**: Auto-detect / Japanese / English
   - **Auto-Start**: Launch at Windows startup
   - **Auto-Update**: Automatic update check (default OFF)

### Exit

Right-click the tray icon → **"Exit"**

## System Requirements

- **OS**: Windows 10 / Windows 11
- **.NET**: 9.0 Runtime (included, no separate installation needed)

## FAQ

**Q: Is installation required?**
A: No. Just double-click the downloaded executable.

**Q: How to uninstall?**
A: Exit the app and delete the executable file. Settings are stored in `%APPDATA%\IMEColorIndicator`.

**Q: Is auto-update safe?**
A: It's disabled by default. When enabled, it only downloads official releases from GitHub Releases.

**Q: Color bars are not showing**
A: Check in Settings that bar display is ON and size is not 0px.

## License

MIT License

## Developer Information

For technical specifications and build instructions, see [DEVMEUS.md](DEVMEUS.md).

---

Created: 2026-01-29

</td>
</tr>
</table>
