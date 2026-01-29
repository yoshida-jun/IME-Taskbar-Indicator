using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IMEColorIndicator;

public partial class SettingsWindow : Window
{
    public System.Windows.Media.Color ImeOffColor { get; private set; }
    public System.Windows.Media.Color ImeOnColor { get; private set; }
    public bool AutoStartEnabled { get; private set; }

    private enum ColorTarget
    {
        ImeOff,
        ImeOn
    }

    private ColorTarget _lastSelectedTarget = ColorTarget.ImeOff;
    private Settings _settings;
    private ColorBarWindow?[] _colorBars;

    public SettingsWindow(
        System.Windows.Media.Color currentOffColor,
        System.Windows.Media.Color currentOnColor,
        bool autoStartEnabled,
        Settings settings,
        ColorBarWindow?[] colorBars)
    {
        InitializeComponent();

        ImeOffColor = currentOffColor;
        ImeOnColor = currentOnColor;
        AutoStartEnabled = autoStartEnabled;
        _settings = settings;
        _colorBars = colorBars;

        // 初期値を設定
        ChkAutoStart.IsChecked = autoStartEnabled;
        ChkShowTop.IsChecked = settings.ShowTopBar;
        ChkShowBottom.IsChecked = settings.ShowBottomBar;
        ChkShowLeft.IsChecked = settings.ShowLeftBar;
        ChkShowRight.IsChecked = settings.ShowRightBar;
        ChkShowTaskbarTop.IsChecked = settings.ShowTaskbarTopBar;

        SliderTopHeight.Value = settings.TopBarHeight;
        SliderBottomHeight.Value = settings.BottomBarHeight;
        SliderLeftWidth.Value = settings.LeftBarWidth;
        SliderRightWidth.Value = settings.RightBarWidth;
        SliderTaskbarTopHeight.Value = settings.TaskbarTopBarHeight;

        UpdatePreview();
        CreatePresetColors();
        UpdateDisplayPreview();

        // ウィンドウを閉じるときに保存
        Closing += (s, e) =>
        {
            AutoStartEnabled = ChkAutoStart.IsChecked ?? false;
            AutoStartHelper.SetAutoStart(AutoStartEnabled);
        };
    }

    private void UpdatePreview()
    {
        ImeOffPreview.Background = new SolidColorBrush(ImeOffColor);
        ImeOnPreview.Background = new SolidColorBrush(ImeOnColor);

        // 選択状態のハイライトを更新
        UpdateSelectionHighlight();

        // リアルタイムでカラーバーを更新
        foreach (var bar in _colorBars)
        {
            bar?.SetColor(ImeOffColor);
        }

        // ディスプレイプレビューの色も更新
        UpdateDisplayPreview();
    }

    private void UpdateDisplayPreview()
    {
        // プレビューコントロールがまだ初期化されていない場合は何もしない
        if (PreviewTopBar == null || PreviewBottomBar == null || PreviewLeftBar == null || PreviewRightBar == null)
            return;

        // 上端バー
        PreviewTopBar.Visibility = (ChkShowTop?.IsChecked ?? true) ? Visibility.Visible : Visibility.Collapsed;
        if (SliderTopHeight != null)
        {
            PreviewTopBar.Height = SliderTopHeight.Value * 0.4; // スケールダウン（実際の40%）
        }
        PreviewTopBar.Background = new SolidColorBrush(ImeOffColor);

        // 下端バー
        PreviewBottomBar.Visibility = (ChkShowBottom?.IsChecked ?? true) ? Visibility.Visible : Visibility.Collapsed;
        if (SliderBottomHeight != null)
        {
            PreviewBottomBar.Height = SliderBottomHeight.Value * 0.4;
        }
        PreviewBottomBar.Background = new SolidColorBrush(ImeOffColor);

        // 左端バー
        PreviewLeftBar.Visibility = (ChkShowLeft?.IsChecked ?? true) ? Visibility.Visible : Visibility.Collapsed;
        if (SliderLeftWidth != null)
        {
            PreviewLeftBar.Width = SliderLeftWidth.Value * 0.4;
        }
        PreviewLeftBar.Background = new SolidColorBrush(ImeOffColor);

        // 右端バー
        PreviewRightBar.Visibility = (ChkShowRight?.IsChecked ?? true) ? Visibility.Visible : Visibility.Collapsed;
        if (SliderRightWidth != null)
        {
            PreviewRightBar.Width = SliderRightWidth.Value * 0.4;
        }
        PreviewRightBar.Background = new SolidColorBrush(ImeOffColor);
    }

    private void CreatePresetColors()
    {
        var presetColors = new[]
        {
            ("DodgerBlue", Colors.DodgerBlue),
            ("LimeGreen", Colors.LimeGreen),
            ("Red", Colors.Red),
            ("Orange", Colors.Orange),
            ("Purple", Colors.Purple),
            ("Pink", Colors.Pink),
            ("Yellow", Colors.Yellow),
            ("Cyan", Colors.Cyan),
            ("Magenta", Colors.Magenta),
            ("Teal", Colors.Teal),
            ("Navy", Colors.Navy),
            ("Crimson", Colors.Crimson),
            ("Gold", Colors.Gold),
            ("Coral", Colors.Coral),
            ("Violet", Colors.Violet),
            ("White", Colors.White),
            ("LightGray", Colors.LightGray),
            ("DarkGray", Colors.DarkGray)
        };

        foreach (var (name, color) in presetColors)
        {
            var button = new System.Windows.Controls.Button
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(color),
                ToolTip = name,
                BorderBrush = System.Windows.Media.Brushes.DarkGray,
                BorderThickness = new Thickness(2),
                Style = (Style)FindResource("ColorButtonStyle")
            };

            button.Click += (s, e) =>
            {
                if (_lastSelectedTarget == ColorTarget.ImeOff)
                {
                    ImeOffColor = color;
                }
                else
                {
                    ImeOnColor = color;
                }
                UpdatePreview();
            };

            PresetColors.Children.Add(button);
        }
    }

    private void BtnSelectOffColor_Click(object sender, RoutedEventArgs e)
    {
        _lastSelectedTarget = ColorTarget.ImeOff;
        var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(ImeOffColor.A, ImeOffColor.R, ImeOffColor.G, ImeOffColor.B),
            FullOpen = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ImeOffColor = System.Windows.Media.Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            UpdatePreview();
        }
    }

    private void BtnSelectOnColor_Click(object sender, RoutedEventArgs e)
    {
        _lastSelectedTarget = ColorTarget.ImeOn;
        var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(ImeOnColor.A, ImeOnColor.R, ImeOnColor.G, ImeOnColor.B),
            FullOpen = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ImeOnColor = System.Windows.Media.Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            UpdatePreview();
        }
    }

    private void ChkShowTop_CheckedChanged(object sender, RoutedEventArgs e)
    {
        _settings.ShowTopBar = ChkShowTop.IsChecked ?? true;
        UpdateDisplayPreview();
    }

    private void ChkShowBottom_CheckedChanged(object sender, RoutedEventArgs e)
    {
        _settings.ShowBottomBar = ChkShowBottom.IsChecked ?? true;
        UpdateDisplayPreview();
    }

    private void ChkShowLeft_CheckedChanged(object sender, RoutedEventArgs e)
    {
        _settings.ShowLeftBar = ChkShowLeft.IsChecked ?? true;
        UpdateDisplayPreview();
    }

    private void ChkShowRight_CheckedChanged(object sender, RoutedEventArgs e)
    {
        _settings.ShowRightBar = ChkShowRight.IsChecked ?? true;
        UpdateDisplayPreview();
    }

    private void SliderTopHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderTopHeight == null || _colorBars == null) return;
        _settings.TopBarHeight = (int)SliderTopHeight.Value;
        _colorBars[0]?.SetSize(_settings.TopBarHeight);
        UpdateDisplayPreview();
    }

    private void SliderBottomHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderBottomHeight == null || _colorBars == null) return;
        _settings.BottomBarHeight = (int)SliderBottomHeight.Value;
        _colorBars[1]?.SetSize(_settings.BottomBarHeight);
        UpdateDisplayPreview();
    }

    private void SliderLeftWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderLeftWidth == null || _colorBars == null) return;
        _settings.LeftBarWidth = (int)SliderLeftWidth.Value;
        _colorBars[2]?.SetSize(_settings.LeftBarWidth);
        UpdateDisplayPreview();
    }

    private void SliderRightWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderRightWidth == null || _colorBars == null) return;
        _settings.RightBarWidth = (int)SliderRightWidth.Value;
        _colorBars[3]?.SetSize(_settings.RightBarWidth);
        UpdateDisplayPreview();
    }

    private void ChkShowTaskbarTop_CheckedChanged(object sender, RoutedEventArgs e)
    {
        _settings.ShowTaskbarTopBar = ChkShowTaskbarTop.IsChecked ?? false;

        // リアルタイムでバーを表示/非表示
        if (_settings.ShowTaskbarTopBar)
        {
            // バーが存在しない場合は新規作成
            if (_colorBars[4] == null && _settings.TaskbarTopBarHeight > 0)
            {
                _colorBars[4] = new ColorBarWindow(ScreenEdge.TaskbarTop, _settings.TaskbarTopBarHeight);
                _colorBars[4].SetColor(ImeOffColor);
            }

            // バーを表示
            _colorBars[4]?.Show();
        }
        else
        {
            // バーを非表示
            _colorBars[4]?.Hide();
        }
    }

    private void SliderTaskbarTopHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderTaskbarTopHeight == null || _colorBars == null) return;
        _settings.TaskbarTopBarHeight = (int)SliderTaskbarTopHeight.Value;
        _colorBars[4]?.SetSize(_settings.TaskbarTopBarHeight);
    }

    private void ImeOffPreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _lastSelectedTarget = ColorTarget.ImeOff;
        UpdateSelectionHighlight();
    }

    private void ImeOnPreview_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _lastSelectedTarget = ColorTarget.ImeOn;
        UpdateSelectionHighlight();
    }

    private void UpdateSelectionHighlight()
    {
        // プレビューコントロールがまだ初期化されていない場合は何もしない
        if (ImeOffPreview == null || ImeOnPreview == null)
            return;

        // IME OFF が選択されている場合
        if (_lastSelectedTarget == ColorTarget.ImeOff)
        {
            ImeOffPreview.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
            ImeOffPreview.BorderThickness = new Thickness(3);

            ImeOnPreview.BorderBrush = System.Windows.Media.Brushes.Gray;
            ImeOnPreview.BorderThickness = new Thickness(1);
        }
        // IME ON が選択されている場合
        else
        {
            ImeOffPreview.BorderBrush = System.Windows.Media.Brushes.Gray;
            ImeOffPreview.BorderThickness = new Thickness(1);

            ImeOnPreview.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
            ImeOnPreview.BorderThickness = new Thickness(3);
        }
    }
}
