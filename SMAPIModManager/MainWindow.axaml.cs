using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SMAPIModManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Console.WriteLine(Convert.ToString(DBConnector.SendSQL("SELECT * FROM Installed")));
    }

    public async void OnButtonPress(object sender, RoutedEventArgs e)
    {
        CurseForgeAPI api = new CurseForgeAPI();
        Console.WriteLine("Button pressed!");
        List<CurseForgeAPI.Mod> mods = await api.GetMods("6");
        api.PopulateScrollViewer(this.FindControl<ScrollViewer>("InstalledMods"), mods);
    }
}