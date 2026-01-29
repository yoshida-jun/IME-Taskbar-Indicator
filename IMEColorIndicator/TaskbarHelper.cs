using System.Runtime.InteropServices;

namespace IMEColorIndicator;

public enum TaskbarPosition
{
    Unknown,
    Left,
    Top,
    Right,
    Bottom
}

public class TaskbarInfo
{
    public TaskbarPosition Position { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public static class TaskbarHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    private const int ABM_GETTASKBARPOS = 5;

    [DllImport("shell32.dll")]
    private static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    public static TaskbarInfo GetTaskbarInfo()
    {
        var data = new APPBARDATA
        {
            cbSize = Marshal.SizeOf(typeof(APPBARDATA))
        };

        var result = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);

        var info = new TaskbarInfo
        {
            X = data.rc.Left,
            Y = data.rc.Top,
            Width = data.rc.Right - data.rc.Left,
            Height = data.rc.Bottom - data.rc.Top
        };

        // タスクバーの位置を判定
        var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

        if (data.rc.Top == data.rc.Left && data.rc.Top == 0 && data.rc.Right >= screenWidth)
        {
            // 上端
            info.Position = TaskbarPosition.Top;
        }
        else if (data.rc.Left == 0 && data.rc.Top == 0 && data.rc.Right < screenWidth)
        {
            // 左端
            info.Position = TaskbarPosition.Left;
        }
        else if (data.rc.Top > 0 && data.rc.Left == 0)
        {
            // 下端
            info.Position = TaskbarPosition.Bottom;
        }
        else if (data.rc.Left > 0)
        {
            // 右端
            info.Position = TaskbarPosition.Right;
        }
        else
        {
            info.Position = TaskbarPosition.Unknown;
        }

        return info;
    }
}
