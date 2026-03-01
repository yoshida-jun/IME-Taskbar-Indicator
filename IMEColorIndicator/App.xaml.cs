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
    private ImeMonitor? _imeMonitor;
    private Settings _settings;
    private SettingsWindow? _settingsWindow;
    private Updater? _updater;

    // 現在の色設定
    private System.Windows.Media.Color _imeOffColor;
    private System.Windows.Media.Color _imeOnColor;

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
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(SystemIcons.Information, 40, 40),
                Visible = true,
                Text = LocalizationHelper.TrayTooltip
            };
            Logger.Log("NotifyIcon created");

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
        if (_settings.ShowTopBar && _settings.TopBarHeight > 0)
        {
            _topBar = new ColorBarWindow(ScreenEdge.Top, _settings.TopBarHeight);
            _topBar.Show();
        }

        if (_settings.ShowBottomBar && _settings.BottomBarHeight > 0)
        {
            _bottomBar = new ColorBarWindow(ScreenEdge.Bottom, _settings.BottomBarHeight);
            _bottomBar.Show();
        }

        if (_settings.ShowLeftBar && _settings.LeftBarWidth > 0)
        {
            _leftBar = new ColorBarWindow(ScreenEdge.Left, _settings.LeftBarWidth);
            _leftBar.Show();
        }

        if (_settings.ShowRightBar && _settings.RightBarWidth > 0)
        {
            _rightBar = new ColorBarWindow(ScreenEdge.Right, _settings.RightBarWidth);
            _rightBar.Show();
        }

        if (_settings.ShowTaskbarTopBar && _settings.TaskbarTopBarHeight > 0)
        {
            _taskbarTopBar = new ColorBarWindow(ScreenEdge.TaskbarTop, _settings.TaskbarTopBarHeight);
            _taskbarTopBar.Show();
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
    }

    private void SetAllBarsColor(System.Windows.Media.Color color)
    {
        _topBar?.SetColor(color);
        _bottomBar?.SetColor(color);
        _leftBar?.SetColor(color);
        _rightBar?.SetColor(color);
        _taskbarTopBar?.SetColor(color);
    }

    private void OnImeStateChanged(object? sender, bool isImeOn)
    {
        SetAllBarsColor(isImeOn ? _imeOnColor : _imeOffColor);
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

            // 設定画面の参照をクリア
            _settingsWindow = null;
        };

        _settingsWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
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
