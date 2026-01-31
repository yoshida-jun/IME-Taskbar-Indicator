using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IMEColorIndicator
{
    public class Updater : IDisposable
    {
        // GitHub リポジトリ情報（必要に応じて変更してください）
        private const string REPO_OWNER = "yoshida-jun";  // TODO: 実際のリポジトリオーナーに変更
        private const string REPO_NAME = "IME-Taskbar-Indicator";  // TODO: 実際のリポジトリ名に変更

        private const string VERSION_URL = $"https://github.com/{REPO_OWNER}/{REPO_NAME}/releases/latest/download/version.txt";
        private const string EXE_URL = $"https://github.com/{REPO_OWNER}/{REPO_NAME}/releases/latest/download/IMEColorIndicator.exe";

        // チェック間隔: 1分（テスト用）、本番環境では30分や1時間に変更推奨
        private static readonly TimeSpan CHECK_INTERVAL = TimeSpan.FromMinutes(1);

        private readonly string _exePath;
        private readonly string _currentVersion;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource? _cts;

        // イベント
        public event EventHandler<string>? UpdateAvailable;
        public event EventHandler? UpdateDownloading;
        public event EventHandler? UpdateApplying;
        public event EventHandler<string>? UpdateFailed;

        public Updater()
        {
            // single-file publish対応: Environment.ProcessPathを使用
            _exePath = Environment.ProcessPath ??
                       Process.GetCurrentProcess().MainModule?.FileName ??
                       throw new InvalidOperationException("実行ファイルパスを取得できませんでした");
            _currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        /// <summary>
        /// バックグラウンドでアップデートチェッカーを起動
        /// </summary>
        public void StartBackgroundChecker()
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () => await BackgroundCheckerLoop(_cts.Token), _cts.Token);
        }

        /// <summary>
        /// バックグラウンドチェッカーを停止
        /// </summary>
        public void StopBackgroundChecker()
        {
            _cts?.Cancel();
        }

        private async Task BackgroundCheckerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(CHECK_INTERVAL, cancellationToken);
                    await CheckAndUpdateAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"アップデートチェックに失敗しました: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// アップデートをチェックし、利用可能な場合は適用
        /// </summary>
        public async Task<bool> CheckAndUpdateAsync()
        {
            try
            {
                // リモートバージョンを取得
                var remoteVersion = await FetchVersionAsync();

                if (!IsNewerVersion(remoteVersion))
                {
                    Debug.WriteLine($"最新バージョンです。現在: {_currentVersion}, リモート: {remoteVersion}");
                    return false;
                }

                Debug.WriteLine($"新しいバージョンが見つかりました: {remoteVersion}");

                // 更新が見つかったことを通知
                UpdateAvailable?.Invoke(this, remoteVersion);

                // ダウンロード開始を通知
                UpdateDownloading?.Invoke(this, EventArgs.Empty);

                // 新しいexeをダウンロード
                var newExeData = await DownloadExeAsync();

                // 更新適用開始を通知
                UpdateApplying?.Invoke(this, EventArgs.Empty);

                // 一時ファイルに保存
                var tempPath = Path.ChangeExtension(_exePath, ".new.exe");
                await File.WriteAllBytesAsync(tempPath, newExeData);

                // アップデートスクリプトを作成して実行
                CreateUpdateScript(tempPath);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"アップデートに失敗しました: {ex.Message}");
                UpdateFailed?.Invoke(this, ex.Message);
                throw;
            }
        }

        private async Task<string> FetchVersionAsync()
        {
            var response = await _httpClient.GetAsync(VERSION_URL);
            response.EnsureSuccessStatusCode();
            var version = await response.Content.ReadAsStringAsync();
            return version.Trim();
        }

        private async Task<byte[]> DownloadExeAsync()
        {
            var response = await _httpClient.GetAsync(EXE_URL);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        private bool IsNewerVersion(string remoteVersion)
        {
            try
            {
                Logger.Log($"[Updater] バージョン比較開始: 現在='{_currentVersion}', リモート='{remoteVersion}'");

                var currentParts = _currentVersion.Split('.');
                var remoteParts = remoteVersion.Split('.');

                Logger.Log($"[Updater] 分割結果: 現在=[{string.Join(",", currentParts)}], リモート=[{string.Join(",", remoteParts)}]");

                for (int i = 0; i < Math.Min(3, Math.Min(currentParts.Length, remoteParts.Length)); i++)
                {
                    if (!int.TryParse(currentParts[i], out int current))
                        current = 0;
                    if (!int.TryParse(remoteParts[i], out int remote))
                        remote = 0;

                    Logger.Log($"[Updater] 比較 [{i}]: current={current}, remote={remote}");

                    if (remote > current)
                    {
                        Logger.Log($"[Updater] 結果: 新しいバージョンあり (remote > current)");
                        return true;
                    }
                    if (remote < current)
                    {
                        Logger.Log($"[Updater] 結果: 現在が新しい (remote < current)");
                        return false;
                    }
                }

                Logger.Log($"[Updater] 結果: 同じバージョン");
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("[Updater] バージョン比較エラー", ex);
                return false;
            }
        }

        private void CreateUpdateScript(string newExePath)
        {
            var scriptPath = Path.ChangeExtension(_exePath, ".update.bat");

            var script = $@"@echo off
chcp 65001 >nul
echo IME Color Indicator を更新しています...
timeout /t 2 /nobreak >nul
del ""{_exePath}""
move ""{newExePath}"" ""{_exePath}""
start """" ""{_exePath}""
del ""%~f0""
";

            File.WriteAllText(scriptPath, script);

            // アップデートスクリプトを実行
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C start /MIN \"\" \"{scriptPath}\"",
                UseShellExecute = true,
                CreateNoWindow = true
            });

            // 現在のプロセスを終了
            Environment.Exit(0);
        }

        public void Dispose()
        {
            _cts?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
