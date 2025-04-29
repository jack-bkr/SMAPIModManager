using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SMAPIModManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CheckCache();
        OnSearchPress(null, null);
    }
    
    public void CheckCache()
    {
        if (!Directory.Exists("cache"))
        {
            Directory.CreateDirectory("cache");
            Directory.CreateDirectory("cache/img");
            Directory.CreateDirectory("cache/mods");
        }
    }
    
    public async void OnSearchPress(object sender, RoutedEventArgs e) 
    {
        CurseForgeAPI api = new CurseForgeAPI();
        Console.WriteLine("Button pressed!");
        List<CurseForgeAPI.Mod> mods = new List<CurseForgeAPI.Mod>();
        
        // Get the sort index and search text
        int sort = this.FindControl<ComboBox>("SortCombo").SelectedIndex;
        string search = this.FindControl<TextBox>("SearchBox").Text;
        
        // Get the mods based on the sort index
        switch (sort)
        {
            case 0:
                mods = await api.GetMods("1", search); 
                break;
            case 1:
                mods = await api.GetMods("2", search);
                break;
            case 2:
                mods = await api.GetMods("6", search);
                break;
            case 3:
                mods = await api.GetMods("12", search);
                break;
            case 4:
                mods = await api.GetMods("3", search);
                break;
        }
        DynamicUI.PopulateScrollViewer(this.FindControl<ScrollViewer>("ModsList"), mods); // Populate the scroll viewer with the mods
    }
    
    async void installMod(object? sender, RoutedEventArgs e)
    {
        string modID;
        Button button = (Button)sender;
        if (button.Name == "btnSMAPI")
        {
            modID = "898372";
        }
        else
        {
            modID = this.FindControl<Grid>("modInfo").Children[0].Name;
        }
        
        CurseForgeAPI api = new CurseForgeAPI();
        CurseForgeAPI.Mod mod = await api.GetMod(modID); // Get the mod object
        
        this.FindControl<Button>("btnInstall").IsEnabled = false;
        this.FindControl<Button>("btnDelete").IsEnabled = true;
        
        Window installDialog = new InstallDialog(mod); // Create the install dialog
        installDialog.Show(); // Show the install dialog
    }
    
    void deleteMod(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Delete button pressed!");
    }

    void installedToggle(object sender, RoutedEventArgs e)
    {
        
    }
    
}