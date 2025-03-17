using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
        
        public Mod(int id, string curseId, string name, string author, string description, string version, string thumbnailUrl)
        {
            this.id = id;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
            try
            {
                this.thumbnailUrl = thumbnailUrl;
            }
            catch (UriFormatException)
            {
                this.thumbnailUrl = "noimg";
            }
        }
        
        public Mod(string curseId, string name, string author, string description, string version, string thumbnailUrl)
        {
            this.id = 0;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
            this.thumbnailUrl = "Assets/flame.png";
            if (thumbnailUrl != "Assets/flame.png")
            {
                this.thumbnailUrl = thumbnailUrl;
            }
        }

        public void Print()
        {
            Console.WriteLine(id);
            Console.WriteLine(curseId);
            Console.WriteLine(name);
            Console.WriteLine(author);
            Console.WriteLine(description);
            Console.WriteLine(version);
            Console.WriteLine();
        }
        
        public async Task<Bitmap?> GetThumbnail()
        {  
            if (File.Exists("cache/img/" + curseId + ".png"))
            {
                return new Bitmap("cache/img/" + curseId + ".png");
            } 
            else if (thumbnailUrl.ToString() == "Assets/flame.png")
            {
                return new Bitmap(AssetLoader.Open(new Uri("avares://SMAPIModManager/Assets/flame.png"), null));
            } 
            else 
            {
                try
                {
                    var response = await client.GetAsync(new Uri(thumbnailUrl));
                    response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsByteArrayAsync();
                    Bitmap bmp = new Bitmap(new MemoryStream(data));
                    bmp.Save("cache/img/" + curseId + ".png");
                    return bmp;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"An error occurred while downloading image '{thumbnailUrl}' : {ex.Message}");
                    return null;
                }
            }
        }
    }
    
    public CurseForgeAPI()
    {
        SetHeaders();
    }
    
    private static void SetHeaders()
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
            HttpResponseMessage response = await client.GetAsync("https://api.curseforge.com/" + URI);
            if (response.IsSuccessStatusCode)
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
    
    public async Task<List<Mod>> GetMods(string sort)
    {
        List<Mod> mods = new List<Mod>();
        string responseBody = await SendRequest("v1/mods/search?gameId=669&sortOrder=desc&sortField=" + sort);
        JsonElement data = JsonDocument.Parse(responseBody).RootElement.GetProperty("data");
        
        foreach (JsonElement mod in data.EnumerateArray())
        {
            string name = mod.GetProperty("name").GetString();
            string author = mod.GetProperty("authors")[0].GetProperty("name").GetString();
            string description = mod.GetProperty("summary").GetString();
            string version = mod.GetProperty("latestFiles")[0].GetProperty("gameVersions")[0].GetString();
            string curseId = mod.GetProperty("id").GetInt32().ToString();
            string thumbnailUrl = "Assets/flame.png";
            if (mod.GetProperty("logo").ValueKind != JsonValueKind.Null)
            {
                thumbnailUrl = mod.GetProperty("logo").GetProperty("thumbnailUrl").GetString();
            } 
            
            mods.Add(new Mod(curseId, name, author, description, version, thumbnailUrl));
        }
        return mods;
    }
    
    public async void PopulateScrollViewer(ScrollViewer scrollViewer, List<Mod> mods)
    {
        StackPanel stackPanel = new StackPanel();
        Boolean alternate = false;

        foreach (Mod mod in mods)
        {
            Grid OuterGrid = new Grid()
            {
                Margin = new Thickness()
            };
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(3, GridUnitType.Star));
            if (alternate)
            {
                OuterGrid.Background = new SolidColorBrush(Colors.LightGray);
                alternate = false;
            }
            else
            {
                alternate = true;
            }
            
            Image thumbnail = new Image()
            {
                Source = await mod.GetThumbnail(),
                Margin = new Thickness(1)
            };
            Grid.SetRow(thumbnail, 0);
            
            Grid InnerGrid = new Grid();
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(InnerGrid, 1);
            
            
            TextBlock Name = new TextBlock()
            {
                Text = mod.name,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Name, 0);
            
            TextBlock Author = new TextBlock()
            {
                Text = mod.author,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Author, 1);
            
            TextBlock Description = new TextBlock()
            {
                Text = mod.description,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Description, 2);
            
            TextBlock Version = new TextBlock()
            {
                Text = mod.version,
                Margin = new Thickness(1)
            };
            Grid.SetRow(Version, 3);
            
            InnerGrid.Children.Add(Name);
            InnerGrid.Children.Add(Author);
            InnerGrid.Children.Add(Description);
            InnerGrid.Children.Add(Version);

            OuterGrid.Children.Add(thumbnail);
            OuterGrid.Children.Add(InnerGrid);
            stackPanel.Children.Add(OuterGrid);
        }
        scrollViewer.Content = stackPanel;
    }
}