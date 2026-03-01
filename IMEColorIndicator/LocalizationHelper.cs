using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace IMEColorIndicator
{
    /// <summary>
    /// 多言語対応のヘルパークラス
    /// </summary>
    public static class LocalizationHelper
    {
        [DllImport("kernel32.dll")]
        private static extern ushort GetUserDefaultUILanguage();

        private const ushort LANG_JAPANESE = 0x0411;

        private static bool _isJapanese = DetectSystemLanguage();
        private static Settings? _currentSettings = null;

        /// <summary>
        /// Settingsを設定（起動時およびSettings変更時に呼び出す）
        /// </summary>
        public static void Initialize(Settings settings)
        {
            _currentSettings = settings;
            UpdateLanguage();
        }

        /// <summary>
        /// 言語設定を更新
        /// </summary>
        private static void UpdateLanguage()
        {
            if (_currentSettings == null)
            {
                _isJapanese = DetectSystemLanguage();
                return;
            }

            _isJapanese = _currentSettings.Language switch
            {
                "Japanese" => true,
                "English" => false,
                _ => DetectSystemLanguage() // "Auto"の場合
            };
        }

        /// <summary>
        /// システム言語が日本語かどうかを判定
        /// </summary>
        private static bool DetectSystemLanguage()
        {
            try
            {
                var langId = GetUserDefaultUILanguage();
                return langId == LANG_JAPANESE;
            }
            catch
            {
                // フォールバック: CultureInfoで判定
                return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja";
            }
        }

        /// <summary>
        /// 現在の言語設定が日本語かどうか
        /// </summary>
        private static bool IsJapanese => _isJapanese;

        // アプリケーション名
        public static string AppName => IsJapanese ? "IME Color Indicator" : "IME Color Indicator";

        // トレイアイコンのツールチップ
        public static string TrayTooltip => "IME Color Indicator";

        // メニュー項目
        public static string MenuSettings => IsJapanese ? "設定" : "Settings";
        public static string MenuExit => IsJapanese ? "終了" : "Exit";

        // 設定ウィンドウ
        public static string SettingsTitle => IsJapanese ? "設定" : "Settings";
        public static string ImeOffColorLabel => IsJapanese ? "IME OFF 時の色:" : "IME OFF Color:";
        public static string ImeOnColorLabel => IsJapanese ? "IME ON 時の色:" : "IME ON Color:";
        public static string AutoStartLabel => IsJapanese ? "Windows起動時に自動起動する" : "Launch at Windows Startup";
        public static string AutoUpdateLabel => IsJapanese ? "自動更新を有効にする" : "Enable Auto Update";
        public static string AutoUpdateEnabled => IsJapanese ? "有効" : "Enabled";
        public static string AutoUpdateDisabled => IsJapanese ? "無効" : "Disabled";
        public static string BarSettingsLabel => IsJapanese ? "バー設定:" : "Bar Settings:";
        public static string ShowTopBar => IsJapanese ? "上端バーを表示" : "Show Top Bar";
        public static string ShowBottomBar => IsJapanese ? "下端バーを表示" : "Show Bottom Bar";
        public static string ShowLeftBar => IsJapanese ? "左端バーを表示" : "Show Left Bar";
        public static string ShowRightBar => IsJapanese ? "右端バーを表示" : "Show Right Bar";
        public static string ShowTaskbarTopBar => IsJapanese ? "タスクバー上端バーを表示" : "Show Taskbar Top Bar";
        public static string BarHeightLabel => IsJapanese ? "高さ (px):" : "Height (px):";
        public static string BarWidthLabel => IsJapanese ? "幅 (px):" : "Width (px):";
        public static string PresetColorsLabel => IsJapanese ? "プリセットカラー:" : "Preset Colors:";
        public static string OkButton => IsJapanese ? "OK" : "OK";
        public static string CancelButton => IsJapanese ? "キャンセル" : "Cancel";

        // カラー名（プリセット）
        public static string ColorRed => IsJapanese ? "赤" : "Red";
        public static string ColorOrange => IsJapanese ? "オレンジ" : "Orange";
        public static string ColorYellow => IsJapanese ? "黄色" : "Yellow";
        public static string ColorGreen => IsJapanese ? "緑" : "Green";
        public static string ColorBlue => IsJapanese ? "青" : "Blue";
        public static string ColorPurple => IsJapanese ? "紫" : "Purple";
        public static string ColorPink => IsJapanese ? "ピンク" : "Pink";
        public static string ColorBrown => IsJapanese ? "茶色" : "Brown";
        public static string ColorGray => IsJapanese ? "グレー" : "Gray";
        public static string ColorBlack => IsJapanese ? "黒" : "Black";
        public static string ColorWhite => IsJapanese ? "白" : "White";
        public static string ColorCyan => IsJapanese ? "シアン" : "Cyan";
        public static string ColorMagenta => IsJapanese ? "マゼンタ" : "Magenta";
        public static string ColorLime => IsJapanese ? "ライム" : "Lime";
        public static string ColorNavy => IsJapanese ? "ネイビー" : "Navy";
        public static string ColorMaroon => IsJapanese ? "マルーン" : "Maroon";
        public static string ColorOlive => IsJapanese ? "オリーブ" : "Olive";
        public static string ColorTeal => IsJapanese ? "ティール" : "Teal";

        // 更新通知
        public static string UpdateAvailable => IsJapanese ? "新しいバージョンが利用可能です" : "A new version is available";
        public static string UpdateDownloading => IsJapanese ? "ダウンロード中..." : "Downloading...";
        public static string UpdateInstalling => IsJapanese ? "インストール中..." : "Installing...";
        public static string UpdateFailed => IsJapanese ? "更新に失敗しました" : "Update failed";
    }
}
