using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace IMEColorIndicator;

public partial class FocusUnderlineWindow : Window
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

    private int _height;

    public FocusUnderlineWindow(int height)
    {
        InitializeComponent();
        _height = height;
        Height = height;

        Loaded += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        };
    }

    public void SetColor(System.Windows.Media.Color color)
    {
        UnderlineBar.Background = new System.Windows.Media.SolidColorBrush(color);
    }

    public void SetHeight(int height)
    {
        _height = height;
    }

    public void UpdatePosition(Rect? elementRect)
    {
        if (elementRect == null || elementRect.Value.IsEmpty)
        {
            Hide();
            return;
        }

        var rect = elementRect.Value;
        Left = rect.Left;
        Top = rect.Bottom;
        Width = rect.Width;
        Height = _height;

        if (!IsVisible) Show();

        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }
}
