using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ChestCounter;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => App.DarkTitleBar(this);

        var s = App.Settings;
        FolderBox.Text = s.ReplayFolder;
        OffsetBox.Text = s.ChestsBefore.ToString();
        ProfileBox.Text = s.ProfileName;
        ObsCheck.IsChecked = s.OverlayEnabled;
        PortBox.Text = s.OverlayPort.ToString();
        StartupCheck.IsChecked = s.StartWithWindows;
        PortBox.TextChanged += (_, _) => UrlText.Text = $"http://localhost:{PortBox.Text}/";
        UrlText.Text = $"http://localhost:{s.OverlayPort}/";
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Choose the Fortnite replay folder", InitialDirectory = FolderBox.Text };
        if (dlg.ShowDialog(this) == true) FolderBox.Text = dlg.FolderName;
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        ImportButton.IsEnabled = false;
        string folder = FolderBox.Text;
        var progress = new Progress<string>(m => ImportStatus.Text = m);
        try { await Task.Run(() => App.Watcher.ImportExisting(folder, progress)); }
        catch (Exception ex) { ImportStatus.Text = "Import failed. See log.txt"; Services.Log.Write(ex.ToString()); }
        ImportButton.IsEnabled = true;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try { Clipboard.SetText(UrlText.Text); } catch { }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(OffsetBox.Text.Trim(), out int offset) || offset < 0)
        {
            MessageBox.Show(this, "Chests before counter must be a whole number, 0 or higher.");
            return;
        }
        if (!int.TryParse(PortBox.Text.Trim(), out int port) || port < 1024 || port > 65535)
        {
            MessageBox.Show(this, "The port must be a number between 1024 and 65535.");
            return;
        }

        var s = App.Settings;
        s.ReplayFolder = FolderBox.Text.Trim();
        s.ChestsBefore = offset;
        s.ProfileName = ProfileBox.Text.Trim();
        s.OverlayEnabled = ObsCheck.IsChecked == true;
        s.OverlayPort = port;
        s.StartWithWindows = StartupCheck.IsChecked == true;
        s.Save();
        App.ApplySettings();
        Close();
    }
}
