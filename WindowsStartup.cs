using System;
using Microsoft.Win32;

namespace ChestCounter.Services;

public static class WindowsStartup
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ChestCounter";

    public static void Set(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath, true);
            if (key == null) return;
            if (enabled && Environment.ProcessPath != null)
                key.SetValue(ValueName, "\"" + Environment.ProcessPath + "\"");
            else
                key.DeleteValue(ValueName, false);
        }
        catch (Exception ex) { Log.Write("Startup entry failed: " + ex.Message); }
    }
}
