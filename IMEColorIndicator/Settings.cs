using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace IMEColorIndicator;

public class Settings
{
    public string ImeOffColor { get; set; } = "#1E90FF"; // DodgerBlue
    public string ImeOnColor { get; set; } = "#32CD32";  // LimeGreen
    public bool AutoStart { get; set; } = false;
    public bool AutoUpdate { get; set; } = false; // 自動更新（デフォルトは無効）
    public string Language { get; set; } = "Auto"; // 言語設定: "Auto", "Japanese", "English"

    // 各辺のバーサイズ
    public int TopBarHeight { get; set; } = 2;     // デフォルト2px
    public int BottomBarHeight { get; set; } = 10; // デフォルト10px
    public int LeftBarWidth { get; set; } = 2;     // デフォルト2px
    public int RightBarWidth { get; set; } = 2;    // デフォルト2px
    public int TaskbarTopBarHeight { get; set; } = 2; // タスクバー上端バーのデフォルト2px

    // 各辺の表示/非表示
    public bool ShowTopBar { get; set; } = true;
    public bool ShowBottomBar { get; set; } = true;
    public bool ShowLeftBar { get; set; } = true;
    public bool ShowRightBar { get; set; } = true;
    public bool ShowTaskbarTopBar { get; set; } = false; // デフォルトは非表示

    // 後方互換性のため残す
    public int BarHeight { get; set; } = 10;

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "IMEColorIndicator",
        "settings.json"
    );

    public static Settings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch
        {
            // エラーが発生した場合はデフォルト設定を返す
        }

        return new Settings();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // エラーが発生しても無視
        }
    }

    public System.Windows.Media.Color GetImeOffColorAsMediaColor()
    {
        return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ImeOffColor);
    }

    public System.Windows.Media.Color GetImeOnColorAsMediaColor()
    {
        return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(ImeOnColor);
    }

    public void SetImeOffColor(System.Windows.Media.Color color)
    {
        ImeOffColor = color.ToString();
    }

    public void SetImeOnColor(System.Windows.Media.Color color)
    {
        ImeOnColor = color.ToString();
    }
}
