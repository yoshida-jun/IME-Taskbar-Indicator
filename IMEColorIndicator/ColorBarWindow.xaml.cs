using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

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
    private DispatcherTimer? _topmostTimer;

    public ColorBarWindow(ScreenEdge edge, int size)
    {
        InitializeComponent();
        _edge = edge;
        _size = size;
        SetupWindow();
        StartTopmostTimer();
    }

    private void SetupWindow()
    {
        // 画面の幅と高さを取得
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        // 辺に応じてウィンドウを配置
        switch (_edge)
        {
            case ScreenEdge.Top:
                Left = 0;
                Top = 0;
                Width = screenWidth;
                Height = _size;
                break;
            case ScreenEdge.Bottom:
                Left = 0;
                Top = screenHeight - _size;
                Width = screenWidth;
                Height = _size;
                break;
            case ScreenEdge.Left:
                Left = 0;
                Top = 0;
                Width = _size;
                Height = screenHeight;
                break;
            case ScreenEdge.Right:
                Left = screenWidth - _size;
                Top = 0;
                Width = _size;
                Height = screenHeight;
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

        // 画面の幅と高さを取得
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        // 辺に応じてウィンドウをリサイズ・再配置
        switch (_edge)
        {
            case ScreenEdge.Top:
                Left = 0;
                Top = 0;
                Width = screenWidth;
                Height = _size;
                break;
            case ScreenEdge.Bottom:
                Left = 0;
                Top = screenHeight - _size;
                Width = screenWidth;
                Height = _size;
                break;
            case ScreenEdge.Left:
                Left = 0;
                Top = 0;
                Width = _size;
                Height = screenHeight;
                break;
            case ScreenEdge.Right:
                Left = screenWidth - _size;
                Top = 0;
                Width = _size;
                Height = screenHeight;
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
        base.OnClosed(e);
    }
}
