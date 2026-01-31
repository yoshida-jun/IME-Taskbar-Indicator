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
    public bool AutoUpdateEnabled { get; private set; }
    public bool ShowOnAllMonitors { get; private set; }
    public bool UseDynamicTrayIcon { get; private set; }
    public bool EnableHotkeys { get; private set; }
    public string ToggleBarsHotkey { get; private set; }
    public string OpenSettingsHotkey { get; private set; }

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
        AutoUpdateEnabled = settings.AutoUpdate;
        ShowOnAllMonitors = settings.ShowOnAllMonitors;
        UseDynamicTrayIcon = settings.UseDynamicTrayIcon;
        EnableHotkeys = settings.EnableHotkeys;
        ToggleBarsHotkey = settings.ToggleBarsHotkey;
        OpenSettingsHotkey = settings.OpenSettingsHotkey;
        _settings = settings;
        _colorBars = colorBars;

        // 初期値を設定
        ChkAutoStart.IsChecked = autoStartEnabled;
        ChkAutoUpdate.IsChecked = settings.AutoUpdate;
        ChkShowOnAllMonitors.IsChecked = settings.ShowOnAllMonitors;
        ChkShowOnAllMonitors.Content = LocalizationHelper.ShowOnAllMonitors;

        // モニター数を表示
        int monitorCount = ColorBarWindow.ScreenCount;
        TxtMonitorCount.Text = string.Format(LocalizationHelper.MonitorCountFormat, monitorCount);

        // トレイアイコン設定
        ChkUseDynamicTrayIcon.IsChecked = settings.UseDynamicTrayIcon;
        ChkUseDynamicTrayIcon.Content = LocalizationHelper.UseDynamicTrayIcon;

        // ホットキー設定
        ChkEnableHotkeys.IsChecked = settings.EnableHotkeys;
        ChkEnableHotkeys.Content = LocalizationHelper.EnableHotkeys;
        TxtToggleBarsHotkeyValue.Text = settings.ToggleBarsHotkey;
        TxtOpenSettingsHotkeyValue.Text = settings.OpenSettingsHotkey;
        TxtToggleBarsHotkey.Text = LocalizationHelper.ToggleBarsHotkey;
        TxtOpenSettingsHotkey.Text = LocalizationHelper.OpenSettingsHotkey;
        TxtHotkeyNote.Text = LocalizationHelper.HotkeyRequiresRestart;

        // 言語設定の初期化
        CmbLanguage.SelectedIndex = settings.Language switch
        {
            "Japanese" => 1,
            "English" => 2,
            _ => 0 // "Auto"
        };

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
            AutoUpdateEnabled = ChkAutoUpdate.IsChecked ?? false;
            ShowOnAllMonitors = ChkShowOnAllMonitors.IsChecked ?? false;
            UseDynamicTrayIcon = ChkUseDynamicTrayIcon.IsChecked ?? true;
            EnableHotkeys = ChkEnableHotkeys.IsChecked ?? false;
            ToggleBarsHotkey = TxtToggleBarsHotkeyValue.Text;
            OpenSettingsHotkey = TxtOpenSettingsHotkeyValue.Text;
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

    private void BtnSelectOffColor_Click(object sender, RoutedEventArgs e) =>
        ShowColorDialog(ColorTarget.ImeOff);

    private void BtnSelectOnColor_Click(object sender, RoutedEventArgs e) =>
        ShowColorDialog(ColorTarget.ImeOn);

    private void ShowColorDialog(ColorTarget target)
    {
        _lastSelectedTarget = target;
        var currentColor = target == ColorTarget.ImeOff ? ImeOffColor : ImeOnColor;
        var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B),
            FullOpen = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var newColor = System.Windows.Media.Color.FromArgb(
                dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            if (target == ColorTarget.ImeOff)
                ImeOffColor = newColor;
            else
                ImeOnColor = newColor;
            UpdatePreview();
        }
    }

    private void ChkShowTop_CheckedChanged(object sender, RoutedEventArgs e) =>
        ToggleBarVisibility(0, ScreenEdge.Top, ChkShowTop.IsChecked ?? true,
            _settings.TopBarHeight, v => _settings.ShowTopBar = v);

    private void ChkShowBottom_CheckedChanged(object sender, RoutedEventArgs e) =>
        ToggleBarVisibility(1, ScreenEdge.Bottom, ChkShowBottom.IsChecked ?? true,
            _settings.BottomBarHeight, v => _settings.ShowBottomBar = v);

    private void ChkShowLeft_CheckedChanged(object sender, RoutedEventArgs e) =>
        ToggleBarVisibility(2, ScreenEdge.Left, ChkShowLeft.IsChecked ?? true,
            _settings.LeftBarWidth, v => _settings.ShowLeftBar = v);

    private void ChkShowRight_CheckedChanged(object sender, RoutedEventArgs e) =>
        ToggleBarVisibility(3, ScreenEdge.Right, ChkShowRight.IsChecked ?? true,
            _settings.RightBarWidth, v => _settings.ShowRightBar = v);

    private void ToggleBarVisibility(int index, ScreenEdge edge, bool show, int size, Action<bool> updateSetting)
    {
        if (_settings == null || _colorBars == null) return;
        updateSetting(show);

        if (show)
        {
            if (_colorBars[index] == null && size > 0)
            {
                var bar = new ColorBarWindow(edge, size);
                bar.SetColor(ImeOffColor);
                _colorBars[index] = bar;
            }
            _colorBars[index]?.Show();
        }
        else
        {
            _colorBars[index]?.Hide();
        }
        UpdateDisplayPreview();
    }

    private void SliderTopHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        UpdateBarSize(0, SliderTopHeight, v => _settings.TopBarHeight = v);

    private void SliderBottomHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        UpdateBarSize(1, SliderBottomHeight, v => _settings.BottomBarHeight = v);

    private void SliderLeftWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        UpdateBarSize(2, SliderLeftWidth, v => _settings.LeftBarWidth = v);

    private void SliderRightWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        UpdateBarSize(3, SliderRightWidth, v => _settings.RightBarWidth = v);

    private void UpdateBarSize(int index, Slider? slider, Action<int> updateSetting)
    {
        if (slider == null || _colorBars == null) return;
        var size = (int)slider.Value;
        updateSetting(size);
        _colorBars[index]?.SetSize(size);
        UpdateDisplayPreview();
    }

    private void ChkShowTaskbarTop_CheckedChanged(object sender, RoutedEventArgs e) =>
        ToggleBarVisibility(4, ScreenEdge.TaskbarTop, ChkShowTaskbarTop.IsChecked ?? false,
            _settings.TaskbarTopBarHeight, v => _settings.ShowTaskbarTopBar = v);

    private void SliderTaskbarTopHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        UpdateBarSize(4, SliderTaskbarTopHeight, v => _settings.TaskbarTopBarHeight = v);

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

    private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 初期化中は処理をスキップ
        if (_settings == null) return;

        if (CmbLanguage.SelectedItem is ComboBoxItem selectedItem)
        {
            var language = selectedItem.Tag?.ToString() ?? "Auto";
            _settings.Language = language;

            // LocalizationHelperを更新
            LocalizationHelper.Initialize(_settings);

            // UIテキストを更新（設定画面を閉じて再度開く必要がある）
            UpdateUITexts();
        }
    }

    private void UpdateUITexts()
    {
        // ウィンドウタイトルを更新
        Title = LocalizationHelper.SettingsTitle;

        // チェックボックスのテキストを更新
        ChkAutoStart.Content = LocalizationHelper.AutoStartLabel;
        ChkAutoUpdate.Content = LocalizationHelper.AutoUpdateLabel;
    }
}
