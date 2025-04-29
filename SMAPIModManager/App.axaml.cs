using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace SMAPIModManager;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Init())
            {
                desktop.MainWindow = new FirstRunWindow();
            } else
            {
                desktop.MainWindow = new MainWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static bool Init()
    {
        Directory.CreateDirectory("cache/img");
        Directory.CreateDirectory("cache/mods");


        if (!File.Exists("DB.db"))
        {
            using (Stream stream = AssetLoader.Open(new Uri("avares://SMAPIModManager/Assets/DB.db")))
            using (Stream output = File.OpenWrite("DB.db"))
            {
                stream.CopyTo(output);
            }
        }
        
        if (!File.Exists("settings.json"))
        {
            using (Stream stream = AssetLoader.Open(new Uri("avares://SMAPIModManager/Assets/settings.json")))
            using (Stream output = File.OpenWrite("settings.json"))
            {
                stream.CopyTo(output);
            }
            return true;
        }
        return false;
    }
}