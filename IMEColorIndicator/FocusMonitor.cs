using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;

namespace IMEColorIndicator;

public class FocusMonitor : IDisposable
{
    public event EventHandler<Rect?>? FocusedElementChanged;

    private AutomationFocusChangedEventHandler? _focusHandler;
    private AutomationElement? _currentElement;
    private DispatcherTimer? _trackTimer;
    private readonly Dispatcher _dispatcher;

    public FocusMonitor(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Start()
    {
        _focusHandler = OnFocusChanged;
        Automation.AddAutomationFocusChangedEventHandler(_focusHandler);

        // 100ms ポーリングで位置を追従（スクロール・ウィンドウ移動に対応）
        _trackTimer = new DispatcherTimer(DispatcherPriority.Normal, _dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _trackTimer.Tick += TrackPosition;
        _trackTimer.Start();
    }

    public void Stop()
    {
        _trackTimer?.Stop();
        _trackTimer = null;

        if (_focusHandler != null)
        {
            Automation.RemoveAutomationFocusChangedEventHandler(_focusHandler);
            _focusHandler = null;
        }
    }

    private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
    {
        var element = sender as AutomationElement;
        if (IsTextInput(element))
        {
            _currentElement = element;
        }
        else
        {
            _currentElement = null;
            _dispatcher.BeginInvoke(() => FocusedElementChanged?.Invoke(this, null));
        }
    }

    private void TrackPosition(object? sender, EventArgs e)
    {
        if (_currentElement == null) return;

        try
        {
            var physicalRect = _currentElement.Current.BoundingRectangle;
            if (physicalRect.IsEmpty || physicalRect.Width <= 0 || physicalRect.Height <= 0)
            {
                _currentElement = null;
                FocusedElementChanged?.Invoke(this, null);
                return;
            }

            var logicalRect = PhysicalToLogical(physicalRect);
            FocusedElementChanged?.Invoke(this, logicalRect);
        }
        catch
        {
            _currentElement = null;
            FocusedElementChanged?.Invoke(this, null);
        }
    }

    private static bool IsTextInput(AutomationElement? element)
    {
        if (element == null) return false;
        try
        {
            var ct = element.Current.ControlType;
            return ct == ControlType.Edit || ct == ControlType.Document;
        }
        catch
        {
            return false;
        }
    }

    private static Rect PhysicalToLogical(Rect physicalRect)
    {
        var scaleX = SystemParameters.PrimaryScreenWidth
                     / System.Windows.Forms.Screen.PrimaryScreen!.Bounds.Width;
        var scaleY = SystemParameters.PrimaryScreenHeight
                     / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        return new Rect(
            physicalRect.Left * scaleX,
            physicalRect.Top * scaleY,
            physicalRect.Width * scaleX,
            physicalRect.Height * scaleY
        );
    }

    public void Dispose() => Stop();
}
