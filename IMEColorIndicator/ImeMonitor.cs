using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace IMEColorIndicator;

public class ImeMonitor
{
    [DllImport("imm32.dll")]
    private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private const uint WM_IME_CONTROL = 0x0283;
    private const uint IMC_GETOPENSTATUS = 0x0005;

    private readonly DispatcherTimer _timer;
    private bool _lastImeState = false;

    public event EventHandler<bool>? ImeStateChanged;

    public ImeMonitor()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _timer.Tick += CheckImeState;
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void CheckImeState(object? sender, EventArgs e)
    {
        var currentState = GetImeState();
        if (currentState != _lastImeState)
        {
            _lastImeState = currentState;
            ImeStateChanged?.Invoke(this, currentState);
        }
    }

    private bool GetImeState()
    {
        try
        {
            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            var imeWindow = ImmGetDefaultIMEWnd(foregroundWindow);
            if (imeWindow == IntPtr.Zero)
                return false;

            var result = SendMessage(imeWindow, WM_IME_CONTROL, new IntPtr(IMC_GETOPENSTATUS), IntPtr.Zero);
            return result.ToInt32() != 0;
        }
        catch
        {
            return false;
        }
    }
}
