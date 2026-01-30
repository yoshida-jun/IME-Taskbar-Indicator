using System;
using System.IO;

namespace IMEColorIndicator
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IMEColorIndicator",
            "debug.log"
        );

        static Logger()
        {
            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // ディレクトリ作成失敗は無視
            }
        }

        public static void Log(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(LogPath, $"[{timestamp}] {message}\n");
            }
            catch
            {
                // ログ書き込み失敗は無視
            }
        }

        public static void LogError(string message, Exception ex)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(LogPath, $"[{timestamp}] ERROR: {message}\n");
                File.AppendAllText(LogPath, $"Exception: {ex.Message}\n");
                File.AppendAllText(LogPath, $"StackTrace: {ex.StackTrace}\n\n");
            }
            catch
            {
                // ログ書き込み失敗は無視
            }
        }

        public static string GetLogPath()
        {
            return LogPath;
        }
    }
}
