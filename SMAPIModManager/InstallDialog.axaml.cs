using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HttpClientProgress;

namespace SMAPIModManager;

public partial class InstallDialog : Window
{
    string cacheFilePath;
    string installFilePath;
    private string modsPath;
    
    public InstallDialog(CurseForgeAPI.Mod mod)
    {
        InitializeComponent();
        
        setModsPath();
        
        this.FindControl<Label>("modLbl").Content = mod.name; // Set the mod name
        downloadFile(mod);
    }
    
    private void setModsPath()
    {
        using (StreamReader sr = new StreamReader("settings.json"))
        {
            string json = sr.ReadToEnd();
            dynamic settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            modsPath = settings["modsDirectory"];
        }
    }
    
    private static readonly HttpClient client = new HttpClient();

    public async Task downloadFile(CurseForgeAPI.Mod mod)
    {
        string URL = mod.downloadUrl;
        Console.WriteLine($"Downloading {URL}"); // Debugging
        
        string fileName = Path.GetFileName(URL);
        cacheFilePath = Path.Combine($"cache/mods/{fileName.Replace("%20", " ")}");
        
        var progress = new Progress<float>(); // Setup progress reporter
        progress.ProgressChanged += Download_ProgressChanged;
        
        using (var file = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {   // use HttpClient Progress extension method
            await client.DownloadDataAsync(URL, file, progress);
        }

        installMod(mod);
    }

    public void installMod(CurseForgeAPI.Mod mod)
    {
        using (ZipArchive archive = ZipFile.OpenRead(cacheFilePath))
        {
            mod.installPath = modsPath + "/" + archive.Entries[0].FullName.Split("/")[0]; // Get the install path from the zip file
            Console.WriteLine($"Installing {cacheFilePath} to {mod.installPath}"); // Debugging
            
            float entryCount = archive.Entries.Count;
            float currentEntry = 0;
            float progress = 0;
            
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string entryPath = Path.Combine(modsPath, entry.FullName);
                
                if (entry.FullName.EndsWith("/"))
                {
                    // If the entry is a directory, create it
                    Directory.CreateDirectory(entryPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));    // Ensure the files directory exists
                    // If the entry is a file, extract it
                    entry.ExtractToFile(entryPath, true);
                }
                
                currentEntry++;
                progress = currentEntry / entryCount * 100;
                Install_ProgressChanged(progress); // Report progress
            
            }
        }
        
        mod.Save();
    }
    
    private void Download_ProgressChanged (object sender, float progress)
    {
        this.FindControl<ProgressBar>("downloadPB").Value = progress; // Update progress bar
        
        Console.WriteLine($"Download progress: {progress}%"); // Debugging
    }
    
    private void Install_ProgressChanged (float progress)
    {
        this.FindControl<ProgressBar>("installPB").Value = progress; // Update progress bar
        Console.WriteLine($"Install progress: {progress}%"); // Debugging
    }
}