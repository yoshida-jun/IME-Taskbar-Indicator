using System.Drawing;
using System.Drawing.Drawing2D;

namespace IMEColorIndicator;

/// <summary>
/// トレイアイコンを動的に生成するヘルパークラス
/// </summary>
public static class TrayIconHelper
{
    private const int IconSize = 32;

    /// <summary>
    /// 指定した色で円形のアイコンを生成
    /// </summary>
    public static Icon CreateColoredIcon(System.Windows.Media.Color mediaColor)
    {
        var color = Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        return CreateColoredIcon(color);
    }

    /// <summary>
    /// 指定した色で円形のアイコンを生成
    /// </summary>
    public static Icon CreateColoredIcon(Color color)
    {
        using var bitmap = new Bitmap(IconSize, IconSize);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        // 外枠を描画（やや暗い色）
        var borderColor = ControlPaint.Dark(color, 0.2f);
        using var borderBrush = new SolidBrush(borderColor);
        graphics.FillEllipse(borderBrush, 1, 1, IconSize - 2, IconSize - 2);

        // 内側を描画
        using var fillBrush = new SolidBrush(color);
        graphics.FillEllipse(fillBrush, 3, 3, IconSize - 6, IconSize - 6);

        // ハイライトを追加
        using var highlightBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
        graphics.FillEllipse(highlightBrush, 6, 4, IconSize / 2, IconSize / 3);

        var handle = bitmap.GetHicon();
        return Icon.FromHandle(handle);
    }

    /// <summary>
    /// カスタムアイコンファイルからアイコンを読み込む
    /// </summary>
    public static Icon? LoadCustomIcon(string? path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return null;

        try
        {
            return new Icon(path);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load custom icon: {path}", ex);
            return null;
        }
    }
}
