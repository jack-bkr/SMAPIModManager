using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace SMAPIModManager;

public partial class MainWindow : Window
{
    bool viewInstalled = false; // Used to toggle between viewing installed mods and CurseForge mods
    
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
    
    public void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) // Check if the enter key was pressed
        {
            OnSearchPress(sender, e);
        }
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
        string modID = this.FindControl<Grid>("modInfo").Children[0].Name;
        ScrollViewer scrollViewer = this.FindControl<ScrollViewer>("ModsList");
        
        List<List<String>> result = DBConnector.SendSQL($"select installPath, Name from Installed where CurseforgeID = \"{modID}\"");
        string installPath = result[0][0];
        string modName = result[0][1];
        
        DBConnector.SendDML($"delete from Installed where CurseforgeID = \"{modID}\""); // Delete the mod from the database
        Directory.Delete(installPath, true);    // Delete the mod's directory
        if (viewInstalled)
        {
            scrollViewer.Content = null;
            DynamicUI.PopulateInstalledMods(scrollViewer); // Refresh the installed mods list
        }
        
        this.FindControl<Button>("btnInstall").IsEnabled = true;
        this.FindControl<Button>("btnDelete").IsEnabled = false;
        
        var box = MessageBoxManager.GetMessageBoxStandard("Caption", $"{modName} has been deleted", ButtonEnum.Ok);    // Show message box
        box.ShowAsync();
    }

    void installedToggle(object sender, RoutedEventArgs e)
    {
        Button toggleBtn = (Button)sender;
        
        if (viewInstalled)
        {
            OnSearchPress( null, null);
            toggleBtn.Content = "View Installed Mods";
            this.FindControl<Button>("btnSearch").IsEnabled = true; // Enable search button for installed mods
            viewInstalled = false;
        }
        else
        {
            DynamicUI.PopulateInstalledMods(this.FindControl<ScrollViewer>("ModsList"));
            toggleBtn.Content = "View CurseForge Mods";
            this.FindControl<Button>("btnSearch").IsEnabled = true; // Disable search button for installed mods
            viewInstalled = true;
        }
    }
    
}