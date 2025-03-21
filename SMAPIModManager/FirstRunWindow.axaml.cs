using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SMAPIModManager;

public partial class FirstRunWindow : Window
{
    public FirstRunWindow()
    {
        InitializeComponent();
    }

    public async void FolderPicker(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        dialog.Title = "Select your SMAPI mods folder";
        var result = await dialog.ShowAsync(this);
        
        if (result != null)
        {
            var textBox = this.FindControl<TextBox>("ModsFolder");
            textBox.Text = result;
        }
    }
    
    public void Confirm(object sender, RoutedEventArgs e)
    {
        using (StreamReader sr = new StreamReader("settings.json"))
        {
            string json = sr.ReadToEnd();
            dynamic settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            settings["modsDirectory"] = this.FindControl<TextBox>("ModsFolder").Text;
            
            string result = JsonSerializer.Serialize(settings);
            File.WriteAllText("settings.json", result);
        }
        
        var mainWindow = new MainWindow();
        mainWindow.Show();

        this.Close();
    }
}