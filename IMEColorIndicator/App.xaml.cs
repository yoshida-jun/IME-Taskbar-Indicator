using System.Windows;
using System.Windows.Forms;

namespace IMEColorIndicator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private NotifyIcon? _notifyIcon;
    private ColorBarWindow? _topBar;
    private ColorBarWindow? _bottomBar;
    private ColorBarWindow? _leftBar;
    private ColorBarWindow? _rightBar;
    private ColorBarWindow? _taskbarTopBar;
    private List<ColorBarWindow> _secondaryMonitorBars = new(); // マルチモニター用
    private ImeMonitor? _imeMonitor;
    private Settings _settings;
    private SettingsWindow? _settingsWindow;
    private Updater? _updater;
    private HotkeyManager? _hotkeyManager;
    private bool _barsVisible = true; // バーの表示状態

    // 現在の色設定
    private System.Windows.Media.Color _imeOffColor;
    private System.Windows.Media.Color _imeOnColor;

    // トレイアイコン（IME状態で切り替え）
    private System.Drawing.Icon? _imeOffIcon;
    private System.Drawing.Icon? _imeOnIcon;
    private bool _currentImeState = false;

    public App()
    {
        try
        {
            Logger.Log("===== App constructor started =====");
            _settings = Settings.Load();
            Logger.Log("Settings loaded");
            LocalizationHelper.Initialize(_settings);
            Logger.Log("LocalizationHelper initialized");
            _imeOffColor = _settings.GetImeOffColorAsMediaColor();
            _imeOnColor = _settings.GetImeOnColorAsMediaColor();
            Logger.Log("App constructor completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("App constructor failed", ex);
            System.Windows.MessageBox.Show(
                $"初期化エラー: {ex.Message}\n\nログ: {Logger.GetLogPath()}\n\n{ex.StackTrace}",
                "IME Color Indicator - エラー",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
            Environment.Exit(1);
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Logger.Log("===== OnStartup started =====");
            // タスクトレイ常駐アプリなので、明示的にShutdownを呼ぶまで終了しない
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Logger.Log("ShutdownMode set");

            // カラーバーウィンドウを作成
            CreateColorBars();
            Logger.Log("ColorBars created");

            // IME監視を開始
            _imeMonitor = new ImeMonitor();
            Logger.Log("ImeMonitor instance created");
            _imeMonitor.ImeStateChanged += OnImeStateChanged;
            Logger.Log("ImeStateChanged event subscribed");
            _imeMonitor.Start();
            Logger.Log("ImeMonitor started");

            // 初期色を設定
            SetAllBarsColor(_imeOffColor);
            Logger.Log("Initial colors set");

            // タスクトレイアイコンを作成
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var versionText = "";
            if (version != null)
            {
                var versionNumber = $"v{version.Major}.{version.Minor}.{version.Build}";
                // Single-file publish（GitHub版）かローカルビルドかを判定
                // Single-file publishの場合、同じフォルダにDLLファイルが存在しない
                var exePath = Environment.ProcessPath ?? "";
                var exeDir = System.IO.Path.GetDirectoryName(exePath) ?? "";
                var hasDll = System.IO.Directory.Exists(exeDir) &&
                             System.IO.Directory.GetFiles(exeDir, "*.dll").Length > 0;
                var buildType = hasDll ? "-dev" : "";
                Logger.Log($"[App] Build type detection: exePath={exePath}, hasDll={hasDll}, buildType={buildType}");
                versionText = $" {versionNumber}{buildType}";
            }

            // トレイアイコンを初期化
            InitializeTrayIcons();

            _notifyIcon = new NotifyIcon
            {
                Icon = _imeOffIcon ?? new System.Drawing.Icon(SystemIcons.Information, 40, 40),
                Visible = true,
                Text = $"{LocalizationHelper.TrayTooltip}{versionText}"
            };
            Logger.Log($"NotifyIcon created with version: {versionText}");

            // 左クリックで設定画面を開く
            _notifyIcon.Click += (s, args) =>
            {
                if (args is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
                {
                    ShowSettings();
                }
            };
            Logger.Log("NotifyIcon Click event subscribed");

            // 右クリックメニューを作成
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(LocalizationHelper.MenuSettings, null, (s, args) => ShowSettings());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(LocalizationHelper.MenuExit, null, (s, args) => Shutdown());

            _notifyIcon.ContextMenuStrip = contextMenu;
            Logger.Log("Context menu created");

            // 自動アップデートチェッカーを起動（設定で有効な場合のみ）
            if (_settings.AutoUpdate)
            {
                try
                {
                    Logger.Log("Starting auto-updater (AutoUpdate enabled)");
                    _updater = new Updater();

                    // 更新イベントをサブスクライブ
                    _updater.UpdateAvailable += OnUpdateAvailable;
                    _updater.UpdateDownloading += OnUpdateDownloading;
                    _updater.UpdateApplying += OnUpdateApplying;
                    _updater.UpdateFailed += OnUpdateFailed;

                    _updater.StartBackgroundChecker();
                    Logger.Log("Auto-updater started");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Auto-updater failed to start", ex);
                    // 更新機能が失敗してもアプリは継続
                }
            }
            else
            {
                Logger.Log("Auto-updater disabled");
            }

            // ホットキーを初期化
            InitializeHotkeys();

            Logger.Log("OnStartup completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("OnStartup failed", ex);
            System.Windows.MessageBox.Show(
                $"起動エラー: {ex.Message}\n\nログ: {Logger.GetLogPath()}\n\n{ex.StackTrace}",
                "IME Color Indicator - エラー",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
            Shutdown();
        }
    }

    private void CreateColorBars()
    {
        // プライマリモニター用バー（screenIndex = 0）
        if (_settings.ShowTopBar && _settings.TopBarHeight > 0)
        {
            _topBar = new ColorBarWindow(ScreenEdge.Top, _settings.TopBarHeight, 0);
            _topBar.Show();
        }

        if (_settings.ShowBottomBar && _settings.BottomBarHeight > 0)
        {
            _bottomBar = new ColorBarWindow(ScreenEdge.Bottom, _settings.BottomBarHeight, 0);
            _bottomBar.Show();
        }

        if (_settings.ShowLeftBar && _settings.LeftBarWidth > 0)
        {
            _leftBar = new ColorBarWindow(ScreenEdge.Left, _settings.LeftBarWidth, 0);
            _leftBar.Show();
        }

        if (_settings.ShowRightBar && _settings.RightBarWidth > 0)
        {
            _rightBar = new ColorBarWindow(ScreenEdge.Right, _settings.RightBarWidth, 0);
            _rightBar.Show();
        }

        if (_settings.ShowTaskbarTopBar && _settings.TaskbarTopBarHeight > 0)
        {
            _taskbarTopBar = new ColorBarWindow(ScreenEdge.TaskbarTop, _settings.TaskbarTopBarHeight, 0);
            _taskbarTopBar.Show();
        }

        // マルチモニター対応: セカンダリモニター用バー
        if (_settings.ShowOnAllMonitors)
        {
            CreateSecondaryMonitorBars();
        }
    }

    private void CreateSecondaryMonitorBars()
    {
        int screenCount = ColorBarWindow.ScreenCount;
        Logger.Log($"[MultiMonitor] Creating bars for {screenCount - 1} secondary monitors");

        for (int i = 1; i < screenCount; i++)
        {
            if (_settings.ShowTopBar && _settings.TopBarHeight > 0)
            {
                var bar = new ColorBarWindow(ScreenEdge.Top, _settings.TopBarHeight, i);
                bar.Show();
                _secondaryMonitorBars.Add(bar);
            }

            if (_settings.ShowBottomBar && _settings.BottomBarHeight > 0)
            {
                var bar = new ColorBarWindow(ScreenEdge.Bottom, _settings.BottomBarHeight, i);
                bar.Show();
                _secondaryMonitorBars.Add(bar);
            }

            if (_settings.ShowLeftBar && _settings.LeftBarWidth > 0)
            {
                var bar = new ColorBarWindow(ScreenEdge.Left, _settings.LeftBarWidth, i);
                bar.Show();
                _secondaryMonitorBars.Add(bar);
            }

            if (_settings.ShowRightBar && _settings.RightBarWidth > 0)
            {
                var bar = new ColorBarWindow(ScreenEdge.Right, _settings.RightBarWidth, i);
                bar.Show();
                _secondaryMonitorBars.Add(bar);
            }
        }
    }

    private void DestroyColorBars()
    {
        _topBar?.Close();
        _topBar = null;

        _bottomBar?.Close();
        _bottomBar = null;

        _leftBar?.Close();
        _leftBar = null;

        _rightBar?.Close();
        _rightBar = null;

        _taskbarTopBar?.Close();
        _taskbarTopBar = null;

        // セカンダリモニター用バーを閉じる
        foreach (var bar in _secondaryMonitorBars)
        {
            bar.Close();
        }
        _secondaryMonitorBars.Clear();
    }

    private void SetAllBarsColor(System.Windows.Media.Color color)
    {
        _topBar?.SetColor(color);
        _bottomBar?.SetColor(color);
        _leftBar?.SetColor(color);
        _rightBar?.SetColor(color);
        _taskbarTopBar?.SetColor(color);

        // セカンダリモニター用バーも更新
        foreach (var bar in _secondaryMonitorBars)
        {
            bar.SetColor(color);
        }
    }

    private void OnImeStateChanged(object? sender, bool isImeOn)
    {
        _currentImeState = isImeOn;
        SetAllBarsColor(isImeOn ? _imeOnColor : _imeOffColor);
        UpdateTrayIcon(isImeOn);
    }

    private void InitializeTrayIcons()
    {
        // カスタムアイコンがあれば使用、なければ色から動的生成
        _imeOffIcon = TrayIconHelper.LoadCustomIcon(_settings.CustomImeOffIconPath)
                      ?? (_settings.UseDynamicTrayIcon ? TrayIconHelper.CreateColoredIcon(_imeOffColor) : null);
        _imeOnIcon = TrayIconHelper.LoadCustomIcon(_settings.CustomImeOnIconPath)
                     ?? (_settings.UseDynamicTrayIcon ? TrayIconHelper.CreateColoredIcon(_imeOnColor) : null);

        Logger.Log($"[TrayIcon] Initialized - UseDynamic={_settings.UseDynamicTrayIcon}, OffIcon={(_imeOffIcon != null)}, OnIcon={(_imeOnIcon != null)}");
    }

    private void UpdateTrayIcon(bool isImeOn)
    {
        if (_notifyIcon == null) return;

        var icon = isImeOn ? _imeOnIcon : _imeOffIcon;
        if (icon != null)
        {
            _notifyIcon.Icon = icon;
        }
    }

    private void InitializeHotkeys()
    {
        if (!_settings.EnableHotkeys)
        {
            Logger.Log("[Hotkey] Hotkeys disabled");
            return;
        }

        try
        {
            _hotkeyManager = new HotkeyManager();
            _hotkeyManager.Initialize();

            _hotkeyManager.ToggleBarsRequested += (s, e) => Dispatcher.Invoke(ToggleBarsVisibility);
            _hotkeyManager.OpenSettingsRequested += (s, e) => Dispatcher.Invoke(ShowSettings);

            bool toggle = _hotkeyManager.RegisterHotkey(HotkeyManager.HOTKEY_TOGGLE_BARS, _settings.ToggleBarsHotkey);
            bool settings = _hotkeyManager.RegisterHotkey(HotkeyManager.HOTKEY_OPEN_SETTINGS, _settings.OpenSettingsHotkey);

            Logger.Log($"[Hotkey] Registration: ToggleBars({_settings.ToggleBarsHotkey})={toggle}, OpenSettings({_settings.OpenSettingsHotkey})={settings}");
        }
        catch (Exception ex)
        {
            Logger.LogError("[Hotkey] Failed to initialize", ex);
        }
    }

    private void ToggleBarsVisibility()
    {
        _barsVisible = !_barsVisible;
        Logger.Log($"[Hotkey] Toggling bars visibility: {_barsVisible}");

        if (_barsVisible)
        {
            _topBar?.Show();
            _bottomBar?.Show();
            _leftBar?.Show();
            _rightBar?.Show();
            _taskbarTopBar?.Show();
            foreach (var bar in _secondaryMonitorBars)
            {
                bar.Show();
            }
        }
        else
        {
            _topBar?.Hide();
            _bottomBar?.Hide();
            _leftBar?.Hide();
            _rightBar?.Hide();
            _taskbarTopBar?.Hide();
            foreach (var bar in _secondaryMonitorBars)
            {
                bar.Hide();
            }
        }
    }

    private void ShowSettings()
    {
        // 既に設定画面が開いている場合はフォーカスを与える
        if (_settingsWindow != null)
        {
            _settingsWindow.Activate();
            _settingsWindow.Focus();
            return;
        }

        var autoStartEnabled = AutoStartHelper.IsAutoStartEnabled();

        _settingsWindow = new SettingsWindow(
            _imeOffColor,
            _imeOnColor,
            autoStartEnabled,
            _settings,
            new[] { _topBar, _bottomBar, _leftBar, _rightBar, _taskbarTopBar }
        );

        // 設定画面が閉じられたときの処理
        _settingsWindow.Closed += (s, e) =>
        {
            // 設定を保存
            _imeOffColor = _settingsWindow.ImeOffColor;
            _imeOnColor = _settingsWindow.ImeOnColor;
            _settings.SetImeOffColor(_imeOffColor);
            _settings.SetImeOnColor(_imeOnColor);
            _settings.AutoStart = _settingsWindow.AutoStartEnabled;
            _settings.ShowOnAllMonitors = _settingsWindow.ShowOnAllMonitors;
            _settings.UseDynamicTrayIcon = _settingsWindow.UseDynamicTrayIcon;
            _settings.EnableHotkeys = _settingsWindow.EnableHotkeys;
            _settings.ToggleBarsHotkey = _settingsWindow.ToggleBarsHotkey;
            _settings.OpenSettingsHotkey = _settingsWindow.OpenSettingsHotkey;

            // 自動更新設定が変更された場合、Updaterを再起動
            var autoUpdateChanged = _settings.AutoUpdate != _settingsWindow.AutoUpdateEnabled;
            _settings.AutoUpdate = _settingsWindow.AutoUpdateEnabled;
            _settings.Save();

            // 言語設定が変更された場合、LocalizationHelperを更新
            LocalizationHelper.Initialize(_settings);

            if (autoUpdateChanged)
            {
                // Updaterを停止
                _updater?.StopBackgroundChecker();
                _updater?.Dispose();
                _updater = null;

                // 自動更新が有効になった場合は起動
                if (_settings.AutoUpdate)
                {
                    try
                    {
                        _updater = new Updater();

                        // 更新イベントをサブスクライブ
                        _updater.UpdateAvailable += OnUpdateAvailable;
                        _updater.UpdateDownloading += OnUpdateDownloading;
                        _updater.UpdateApplying += OnUpdateApplying;
                        _updater.UpdateFailed += OnUpdateFailed;

                        _updater.StartBackgroundChecker();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"自動更新の起動に失敗しました: {ex.Message}");
                    }
                }
            }

            // バーを再作成
            DestroyColorBars();
            CreateColorBars();
            SetAllBarsColor(_imeOffColor);

            // トレイアイコンを再初期化
            InitializeTrayIcons();
            UpdateTrayIcon(_currentImeState);

            // 設定画面の参照をクリア
            _settingsWindow = null;
        };

        _settingsWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyManager?.Dispose();
        _imeMonitor?.Stop();
        _updater?.StopBackgroundChecker();
        _updater?.Dispose();
        _notifyIcon?.Dispose();
        DestroyColorBars();
        base.OnExit(e);
    }

    // 更新イベントハンドラー
    private void OnUpdateAvailable(object? sender, string version)
    {
        Dispatcher.Invoke(() =>
        {
            _notifyIcon?.ShowBalloonTip(
                3000,
                "IME Color Indicator",
                $"{LocalizationHelper.UpdateAvailable}: v{version}",
                ToolTipIcon.Info
            );
        });
    }

    private void OnUpdateDownloading(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _notifyIcon?.ShowBalloonTip(
                3000,
                "IME Color Indicator",
                LocalizationHelper.UpdateDownloading,
                ToolTipIcon.Info
            );
        });
    }

    private void OnUpdateApplying(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _notifyIcon?.ShowBalloonTip(
                3000,
                "IME Color Indicator",
                LocalizationHelper.UpdateInstalling,
                ToolTipIcon.Info
            );
        });
    }

    private void OnUpdateFailed(object? sender, string error)
    {
        Dispatcher.Invoke(() =>
        {
            _notifyIcon?.ShowBalloonTip(
                5000,
                "IME Color Indicator",
                $"{LocalizationHelper.UpdateFailed}: {error}",
                ToolTipIcon.Error
            );
        });
    }
}
