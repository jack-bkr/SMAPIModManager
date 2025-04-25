using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HttpClientProgress;

namespace SMAPIModManager;

public partial class InstallDialog : Window
{
    public InstallDialog(CurseForgeAPI.Mod mod)
    {
        InitializeComponent();
        
        this.FindControl<Label>("modLbl").Content = mod.name; // Set the mod name
        downloadFile(mod.downloadUrl);
    }
    
    private static readonly HttpClient client = new HttpClient();

    public async Task downloadFile(string URL)
    {
        string fileName = Path.GetFileName(URL);
        var filePath = Path.Combine($"cache/mods/{fileName}");
        
        var progress = new Progress<float>(); // Setup progress reporter
        progress.ProgressChanged += Download_ProgressChanged;
        
        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {   // use HttpClient Progress extension method
            await client.DownloadDataAsync(URL, file, progress);
        }
    }

    private void Download_ProgressChanged (object sender, float progress)
    {
        this.FindControl<ProgressBar>("downloadPB").Value = progress; // Update progress bar
    }
}