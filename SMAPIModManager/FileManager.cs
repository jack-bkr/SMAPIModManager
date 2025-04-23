using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientProgress;

namespace SMAPIModManager;

public class FileManager
{
    private static readonly HttpClient client = new HttpClient();

    async public static Task downloadFile(string URL, string fileName)
    {
        var filePath = Path.Combine($"cache/mods/{fileName}");
        
        var progress = new Progress<float>(); // Setup progress reporter
        progress.ProgressChanged += Progress_ProgressChanged;
        
        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {   // use HttpClient Progress extension method
            await client.DownloadDataAsync(URL, file, progress);
        }
    }

    static void Progress_ProgressChanged (object sender, float progress)
    {
        // Do something with your progress
        Console.WriteLine (progress);
    }
}