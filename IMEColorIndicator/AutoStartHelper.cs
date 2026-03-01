using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace IMEColorIndicator;

public static class AutoStartHelper
{
    private const string AppName = "IMEColorIndicator";
    private static readonly string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                // single-file publish対応: Environment.ProcessPathを使用
                var exePath = Environment.ProcessPath ??
                              Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                if (string.IsNullOrEmpty(exePath)) return;

                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // エラーが発生しても無視
        }
    }
}
