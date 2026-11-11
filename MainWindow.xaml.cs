using System;
using System.Windows;
using System.Windows.Media;

namespace ChestCounter;

public partial class MainWindow : Window
{
    private OverlayWindow _overlay;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => App.DarkTitleBar(this);
        Loaded += (_, _) => { App.Store.Changed += OnChanged; Refresh(); };
        Closed += (_, _) => App.Store.Changed -= OnChanged;
    }

    private void OnChanged() => Dispatcher.BeginInvoke(new Action(Refresh));

    public void Refresh()
    {
        DataContext = App.Store.Snapshot(App.Settings);
        bool ok = App.Watcher.Watching;
        StatusText.Text = ok ? "Watching for new matches" : "Replay folder not found. Check Settings.";
        StatusText.Foreground = new SolidColorBrush(ok ? Color.FromRgb(0x4A, 0xDE, 0x80) : Color.FromRgb(0xF8, 0x71, 0x71));
    }

    private void Overlay_Click(object sender, RoutedEventArgs e)
    {
        if (_overlay != null && _overlay.IsLoaded)
        {
            _overlay.Close();
            _overlay = null;
            return;
        }
        _overlay = new OverlayWindow { Owner = null };
        _overlay.Show();
    }

    private void Report_Click(object sender, RoutedEventArgs e) => new ReportWindow { Owner = this }.ShowDialog();

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        new SettingsWindow { Owner = this }.ShowDialog();
        Refresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        _overlay?.Close();
        base.OnClosed(e);
    }
}
