using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;

namespace IMEColorIndicator;

public enum ScreenEdge
{
    Top,
    Bottom,
    Left,
    Right,
    TaskbarTop  // タスクバーの上端
}

public partial class ColorBarWindow : Window
{
    public static int ScreenCount => System.Windows.Forms.Screen.AllScreens.Length;

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private ScreenEdge _edge;
    private int _size; // 幅または高さ（ピクセル）
    private int _screenIndex; // モニター番号 (0 = プライマリ)
    private DispatcherTimer? _topmostTimer;

    public ColorBarWindow(ScreenEdge edge, int size, int screenIndex = 0)
    {
        InitializeComponent();
        _edge = edge;
        _size = size;
        _screenIndex = screenIndex;
        SetupWindow();
        StartTopmostTimer();

        // 解像度変更を監視
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    private System.Drawing.Rectangle GetScreenBounds()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (_screenIndex >= 0 && _screenIndex < screens.Length)
        {
            return screens[_screenIndex].Bounds;
        }
        return System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // UIスレッドで実行
        Dispatcher.BeginInvoke(() =>
        {
            Logger.Log($"[ColorBarWindow] 解像度変更を検出: 再配置します");
            SetSize(_size);
        });
    }

    private void SetupWindow()
    {
        // スクリーン情報を取得
        var bounds = GetScreenBounds();

        // 辺に応じてウィンドウを配置
        switch (_edge)
        {
            case ScreenEdge.Top:
                Left = bounds.X;
                Top = bounds.Y;
                Width = bounds.Width;
                Height = _size;
                break;
            case ScreenEdge.Bottom:
                Left = bounds.X;
                Top = bounds.Y + bounds.Height - _size;
                Width = bounds.Width;
                Height = _size;
                break;
            case ScreenEdge.Left:
                Left = bounds.X;
                Top = bounds.Y;
                Width = _size;
                Height = bounds.Height;
                break;
            case ScreenEdge.Right:
                Left = bounds.X + bounds.Width - _size;
                Top = bounds.Y;
                Width = _size;
                Height = bounds.Height;
                break;
            case ScreenEdge.TaskbarTop:
                var taskbarInfo = TaskbarHelper.GetTaskbarInfo();
                // タスクバーの上端に表示
                Left = taskbarInfo.X;
                Top = taskbarInfo.Y;
                Width = taskbarInfo.Width;
                Height = _size;
                break;
        }

        // ウィンドウがロードされたらクリック透過を設定
        Loaded += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);

            // 常に最前面に表示
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        };
    }

    public void SetSize(int size)
    {
        _size = size;

        // スクリーン情報を取得
        var bounds = GetScreenBounds();

        // 辺に応じてウィンドウをリサイズ・再配置
        switch (_edge)
        {
            case ScreenEdge.Top:
                Left = bounds.X;
                Top = bounds.Y;
                Width = bounds.Width;
                Height = _size;
                break;
            case ScreenEdge.Bottom:
                Left = bounds.X;
                Top = bounds.Y + bounds.Height - _size;
                Width = bounds.Width;
                Height = _size;
                break;
            case ScreenEdge.Left:
                Left = bounds.X;
                Top = bounds.Y;
                Width = _size;
                Height = bounds.Height;
                break;
            case ScreenEdge.Right:
                Left = bounds.X + bounds.Width - _size;
                Top = bounds.Y;
                Width = _size;
                Height = bounds.Height;
                break;
            case ScreenEdge.TaskbarTop:
                var taskbarInfo = TaskbarHelper.GetTaskbarInfo();
                // タスクバーの上端に表示
                Left = taskbarInfo.X;
                Top = taskbarInfo.Y;
                Width = taskbarInfo.Width;
                Height = _size;
                break;
        }

        // 常に最前面に表示
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    public void SetColor(System.Windows.Media.Color color)
    {
        ColorBar.Background = new System.Windows.Media.SolidColorBrush(color);
    }

    private void StartTopmostTimer()
    {
        // 定期的に最前面に保つタイマー（500ms間隔）
        _topmostTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _topmostTimer.Tick += (s, e) => EnsureTopmost();
        _topmostTimer.Start();
    }

    private void EnsureTopmost()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _topmostTimer?.Stop();
        _topmostTimer = null;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        base.OnClosed(e);
    }
}
