using System;
using System.Windows;
using System.Windows.Input;

namespace ChestCounter;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => { App.Store.Changed += OnChanged; Refresh(); };
        Closed += (_, _) => App.Store.Changed -= OnChanged;
    }

    private void OnChanged() => Dispatcher.BeginInvoke(new Action(Refresh));
    private void Refresh() => DataContext = App.Store.Snapshot(App.Settings);

    private void Drag(object sender, MouseButtonEventArgs e)
    {
        try { DragMove(); } catch { }
    }

    private void Close_Click(object sender, MouseButtonEventArgs e) => Close();
}
