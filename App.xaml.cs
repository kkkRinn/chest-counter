using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ChestCounter.Models;
using ChestCounter.Services;

namespace ChestCounter;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; }
    public static HistoryStore Store { get; private set; }
    public static ReplayWatcher Watcher { get; private set; }
    public static OverlayServer Overlay { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, ex) =>
        {
            Log.Write("Unhandled: " + ex.Exception);
            ex.Handled = true;
        };

        Settings = AppSettings.Load();
        Store = new HistoryStore();
        Store.Load();
        Overlay = new OverlayServer(() => Store.Snapshot(Settings));
        Watcher = new ReplayWatcher(Store);
        ApplySettings();

        new MainWindow().Show();
    }

    /// <summary>Re-applies settings to the watcher, OBS server and startup entry.</summary>
    public static void ApplySettings()
    {
        Watcher.Start(Settings.ReplayFolder);
        if (Settings.OverlayEnabled) Overlay.Start(Settings.OverlayPort);
        else Overlay.Stop();
        WindowsStartup.Set(Settings.StartWithWindows);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Watcher?.Dispose();
        Overlay?.Dispose();
        base.OnExit(e);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    /// <summary>Makes the native title bar dark (Windows 10 20H1+ / 11).</summary>
    public static void DarkTitleBar(Window w)
    {
        try
        {
            var h = new WindowInteropHelper(w).Handle;
            int on = 1;
            DwmSetWindowAttribute(h, 20, ref on, sizeof(int));
        }
        catch { }
    }
}
