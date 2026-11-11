using System.Diagnostics;
using System.Windows;
using ChestCounter.Services;

namespace ChestCounter;

public partial class ReportWindow : Window
{
    public ReportWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => App.DarkTitleBar(this);
        Grid.ItemsSource = App.Store.All();
    }

    private void Folder_Click(object sender, RoutedEventArgs e)
    {
        System.IO.Directory.CreateDirectory(Log.Dir);
        Process.Start(new ProcessStartInfo(Log.Dir) { UseShellExecute = true });
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        var ok = MessageBox.Show(this, "Delete all recorded matches? Your 'chests before counter' offset is kept.",
            "Clear history", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (ok != MessageBoxResult.Yes) return;
        App.Store.Clear();
        Grid.ItemsSource = App.Store.All();
    }
}
