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
                var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
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
