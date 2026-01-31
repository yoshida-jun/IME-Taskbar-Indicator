using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace IMEColorIndicator;

/// <summary>
/// グローバルホットキーを管理するクラス
/// </summary>
public class HotkeyManager : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // 修飾キー
    public const uint MOD_NONE = 0x0000;
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    // 仮想キーコード
    private static readonly Dictionary<string, uint> VirtualKeyCodes = new()
    {
        { "F1", 0x70 }, { "F2", 0x71 }, { "F3", 0x72 }, { "F4", 0x73 },
        { "F5", 0x74 }, { "F6", 0x75 }, { "F7", 0x76 }, { "F8", 0x77 },
        { "F9", 0x78 }, { "F10", 0x79 }, { "F11", 0x7A }, { "F12", 0x7B },
        { "A", 0x41 }, { "B", 0x42 }, { "C", 0x43 }, { "D", 0x44 },
        { "E", 0x45 }, { "F", 0x46 }, { "G", 0x47 }, { "H", 0x48 },
        { "I", 0x49 }, { "J", 0x4A }, { "K", 0x4B }, { "L", 0x4C },
        { "M", 0x4D }, { "N", 0x4E }, { "O", 0x4F }, { "P", 0x50 },
        { "Q", 0x51 }, { "R", 0x52 }, { "S", 0x53 }, { "T", 0x54 },
        { "U", 0x55 }, { "V", 0x56 }, { "W", 0x57 }, { "X", 0x58 },
        { "Y", 0x59 }, { "Z", 0x5A },
        { "0", 0x30 }, { "1", 0x31 }, { "2", 0x32 }, { "3", 0x33 },
        { "4", 0x34 }, { "5", 0x35 }, { "6", 0x36 }, { "7", 0x37 },
        { "8", 0x38 }, { "9", 0x39 },
    };

    private const int WM_HOTKEY = 0x0312;

    // ホットキーID
    public const int HOTKEY_TOGGLE_BARS = 1;
    public const int HOTKEY_OPEN_SETTINGS = 2;

    private MessageWindow? _messageWindow;
    private bool _disposed;

    public event EventHandler? ToggleBarsRequested;
    public event EventHandler? OpenSettingsRequested;

    public HotkeyManager()
    {
    }

    public void Initialize()
    {
        _messageWindow = new MessageWindow(this);
    }

    public bool RegisterHotkey(int id, string hotkeyString)
    {
        if (_messageWindow == null) return false;

        var (modifiers, key) = ParseHotkeyString(hotkeyString);
        if (key == 0) return false;

        return RegisterHotKey(_messageWindow.Handle, id, modifiers | MOD_NOREPEAT, key);
    }

    public void UnregisterAllHotkeys()
    {
        if (_messageWindow == null) return;
        UnregisterHotKey(_messageWindow.Handle, HOTKEY_TOGGLE_BARS);
        UnregisterHotKey(_messageWindow.Handle, HOTKEY_OPEN_SETTINGS);
    }

    internal void OnHotkey(int id)
    {
        switch (id)
        {
            case HOTKEY_TOGGLE_BARS:
                ToggleBarsRequested?.Invoke(this, EventArgs.Empty);
                break;
            case HOTKEY_OPEN_SETTINGS:
                OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private static (uint modifiers, uint key) ParseHotkeyString(string hotkeyString)
    {
        uint modifiers = MOD_NONE;
        uint key = 0;

        var parts = hotkeyString.ToUpper().Split('+');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            switch (trimmed)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    if (VirtualKeyCodes.TryGetValue(trimmed, out var vk))
                    {
                        key = vk;
                    }
                    break;
            }
        }

        return (modifiers, key);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        UnregisterAllHotkeys();
        _messageWindow?.Dispose();
    }

    /// <summary>
    /// ホットキーメッセージを受け取るための非表示ウィンドウ
    /// </summary>
    private class MessageWindow : IDisposable
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private HwndSource? _source;
        private readonly HotkeyManager _parent;

        public IntPtr Handle => _source?.Handle ?? IntPtr.Zero;

        public MessageWindow(HotkeyManager parent)
        {
            _parent = parent;
            var parameters = new HwndSourceParameters("IMEColorIndicatorHotkeyWindow")
            {
                Width = 0,
                Height = 0,
                WindowStyle = 0,
                ExtendedWindowStyle = WS_EX_TOOLWINDOW
            };

            _source = new HwndSource(parameters);
            _source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                _parent.OnHotkey(id);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _source?.RemoveHook(WndProc);
            _source?.Dispose();
            _source = null;
        }
    }
}
