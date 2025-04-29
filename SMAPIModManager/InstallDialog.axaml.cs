using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using HttpClientProgress;

namespace SMAPIModManager;

public partial class InstallDialog : Window
{
    string cacheFilePath;
    string installFilePath;
    private string modsPath;
    private static readonly HttpClient client = new HttpClient();
    
    public InstallDialog(CurseForgeAPI.Mod mod)
    {
        InitializeComponent();
        Run(mod);
    }

    private async void Run(CurseForgeAPI.Mod mod)
    {
        setModsPath();
        
        this.FindControl<Label>("modLbl").Content = mod.name; // Set the mod name
        await downloadFile(mod);
        installMod(mod);

        if (mod.curseId == "898372")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(Directory.GetCurrentDirectory() + mod.installPath + "install on Linux.sh");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start(mod.installPath + "install on macOS.command");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(mod.installPath + "install on Windows.bat");
            }
        }
        else
        {
            mod.Save();
        }
        

        await Task.Delay(400);
        this.Close();
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

    private async Task downloadFile(CurseForgeAPI.Mod mod)
    {
        string URL = mod.downloadUrl;
        Console.WriteLine($"Downloading {URL}"); // Debugging
        
        string fileName = Path.GetFileName(URL);
        cacheFilePath = Path.Combine($"cache/mods/{fileName.Replace("%20", " ")}");

        if (!File.Exists(cacheFilePath))    // Check if file already exists, skip download if it does
        {
            var progress = new Progress<float>(); // Setup progress reporter
            progress.ProgressChanged += Download_ProgressChanged;

            using (var file = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // use HttpClient Progress extension method
                await client.DownloadDataAsync(URL, file, progress);
            }
        }
    }

    private void installMod(CurseForgeAPI.Mod mod)
    {
        using (ZipArchive archive = ZipFile.OpenRead(cacheFilePath))
        {
            if (mod.curseId == "898372")    // if SMAPI install to cache
            {
                mod.installPath = "/cache/" + archive.Entries[0].FullName.Split("/")[0] + "/";
            }
            else
            {
                mod.installPath = modsPath + "/" + archive.Entries[0].FullName.Split("/")[0]; // Get the install path from the zip file
            }
            
            Console.WriteLine($"Installing {cacheFilePath} to {mod.installPath}"); // Debugging
            
            float entryCount = archive.Entries.Count;
            float currentEntry = 0;
            float progress = 0;
            
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string entryPath;
                
                if (mod.curseId == "898372")    // check if mod is SMAPI
                {
                    entryPath = "cache/" + entry.FullName;
                }
                else
                {
                    entryPath = Path.Combine(modsPath, entry.FullName);
                }

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