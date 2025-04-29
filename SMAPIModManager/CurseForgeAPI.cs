using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SMAPIModManager;

public class CurseForgeAPI
{
    private static readonly HttpClient client = new HttpClient();

    public class Mod : CurseForgeAPI
    {
        public int id { get; set; }
        public string curseId { get; set; }
        public string name { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string thumbnailUrl { get; set; }
        public string downloadUrl { get; set; }
        public string installPath { get; set; }
        
        public Mod(int id, string curseId, string name, string author, string description, string version, string thumbnailUrl, string downloadUrl, string installPath)
        {
            this.id = id;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
            this.thumbnailUrl = thumbnailUrl;
            this.downloadUrl = downloadUrl;
            this.installPath = installPath;
        }
        
        public Mod(string curseId, string name, string author, string description, string version, string thumbnailUrl, string downloadUrl)
        {
            this.id = 0;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
            this.thumbnailUrl = "Assets/flame.png"; // Default thumbnail
            if (thumbnailUrl != "Assets/flame.png")
            {
                this.thumbnailUrl = thumbnailUrl;
            }
            this.downloadUrl = downloadUrl;
            this.installPath = "";
        }

        public void Print() // Debugging method
        {
            Console.WriteLine(id);
            Console.WriteLine(curseId);
            Console.WriteLine(name);
            Console.WriteLine(author);
            Console.WriteLine(description);
            Console.WriteLine(version);
            Console.WriteLine(thumbnailUrl);
            Console.WriteLine(downloadUrl);
            Console.WriteLine();
        }
        
        public async Task<Bitmap> GetThumbnail() // Get the thumbnail image
        {  
            if (File.Exists("cache/img/" + curseId + ".png")) // Check if the image is in the cache
            {
                return new Bitmap("cache/img/" + curseId + ".png");
            } 
            else if (thumbnailUrl == "Assets/flame.png") // Check if the thumbnail is the default thumbnail
            {
                return new Bitmap(AssetLoader.Open(new Uri("avares://SMAPIModManager/Assets/flame.png"), null));
            } 
            else 
            {
                try
                {
                    var response = await client.GetAsync(new Uri(thumbnailUrl)); // Download the image
                    response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsByteArrayAsync();
                    using (var memoryStream = new MemoryStream(data))
                    using (var fileStream = new FileStream($"./cache/img/{curseId}.png", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        Bitmap bmp = new Bitmap(memoryStream);
                        bmp.Save(fileStream); // Save the image to the cache
                        return bmp;
                    }
                }
                catch (HttpRequestException ex) // Catch any HTTP errors
                {
                    Console.WriteLine($"An error occurred while downloading image '{thumbnailUrl}' : {ex.Message}");
                    return null;
                }
            }
        }

        public void Save()
        {
            string query = "INSERT INTO Installed (CurseforgeID, Name, Author, Description, Version, thumbnailUrl, downloadUrl) " +
                           $"VALUES (\"{curseId}\", \"{name}\", \"{author}\", \"{description}\", \"{version}\", \"{thumbnailUrl}\", \"{downloadUrl}\");";
        
            DBConnector.SendDML(query);
        }
    }
    
    public CurseForgeAPI()
    {
        SetHeaders();
    }
    
    private static void SetHeaders() // Set the headers for the HTTP client
    {
        using (StreamReader sr = new StreamReader("settings.json"))
        {
            string json = sr.ReadToEnd();
            dynamic settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", settings["api-key"].ToString());
        }
    }
    public async Task<string> SendRequest(string URI)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("https://api.curseforge.com/" + URI); // Send the request
            if (response.IsSuccessStatusCode) // Check if the request was successful
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                return "Error: " + response.StatusCode;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }

        return "";
    }
    
    public async Task<List<Mod>> GetMods(string sort, string search)
    {
        List<Mod> mods = new List<Mod>();
        string responseBody = await SendRequest("v1/mods/search?gameId=669&sortOrder=desc&sortField=" + sort + "&searchFilter=" + search);
        JsonElement data = JsonDocument.Parse(responseBody).RootElement.GetProperty("data"); // Parse the JSON response
        
        foreach (JsonElement mod in data.EnumerateArray()) // Loop through the mods
        {
            mods.Add(FormatMod(mod)); // Add the mod to the list
        }
        return mods;
    }
    
    public async Task<Mod> GetMod(string modId)
    {
        string responseBody = await SendRequest("v1/mods/" + modId);
        JsonElement data = JsonDocument.Parse(responseBody).RootElement.GetProperty("data"); // Parse the JSON response
        
        return FormatMod(data); // Return the mod
    }

    public Mod FormatMod(JsonElement mod)   // Extract data from JSON response and return a new mod
    {
        string name = mod.GetProperty("name").GetString();
        string author = mod.GetProperty("authors")[0].GetProperty("name").GetString();
        string description = mod.GetProperty("summary").GetString();
        string version = mod.GetProperty("latestFiles")[0].GetProperty("gameVersions")[0].GetString();
        string curseId = mod.GetProperty("id").GetInt32().ToString();
        string thumbnailUrl = "Assets/flame.png";
        if (mod.GetProperty("logo").ValueKind != JsonValueKind.Null) // Check if the mod has a thumbnail
        {
            thumbnailUrl = mod.GetProperty("logo").GetProperty("thumbnailUrl").GetString();
        }
        string downloadUrl = mod.GetProperty("latestFiles")[0].GetProperty("downloadUrl").GetString();
        
        return new Mod(curseId, name, author, description, version, thumbnailUrl, downloadUrl); // Return the mod
    }
}