using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
            this.thumbnailUrl = thumbnailUrl;
        }
        
        public Mod(string curseId, string name, string author, string description, string version, string thumbnailUrl)
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
        }

        public void Print() // Debugging method
        {
            Console.WriteLine(id);
            Console.WriteLine(curseId);
            Console.WriteLine(name);
            Console.WriteLine(author);
            Console.WriteLine(description);
            Console.WriteLine(version);
            Console.WriteLine();
        }
        
        public async Task<Bitmap?> GetThumbnail() // Get the thumbnail image
        {  
            if (File.Exists("cache/img/" + curseId + ".png")) // Check if the image is in the cache
            {
                return new Bitmap("cache/img/" + curseId + ".png");
            } 
            else if (thumbnailUrl.ToString() == "Assets/flame.png") // Check if the thumbnail is the default thumbnail
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
                    Bitmap bmp = new Bitmap(new MemoryStream(data));
                    bmp.Save("cache/img/" + curseId + ".png"); // Save the image to the cache
                    return bmp;
                }
                catch (HttpRequestException ex) // Catch any HTTP errors
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
            
            mods.Add(new Mod(curseId, name, author, description, version, thumbnailUrl)); // Add the mod to the list
        }
        return mods;
    }
    
    public async void PopulateScrollViewer(ScrollViewer scrollViewer, List<Mod> mods)
    {
        StackPanel stackPanel = new StackPanel();
        Boolean alternate = false;

        foreach (Mod mod in mods) // Loop through the mods
        {
            Grid OuterGrid = new Grid() // Create the outer grid, each outer grid is 1 mod
            {
                Name = mod.curseId,
                Margin = new Thickness(),
                MaxHeight = 100
            };
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            OuterGrid.ColumnDefinitions.Add(new ColumnDefinition(3, GridUnitType.Star));
            OuterGrid.AddHandler(Grid.PointerPressedEvent, onModClick);
            
            Image thumbnail = new Image()
            {
                Source = await mod.GetThumbnail(),
                Margin = new Thickness(2,2,0,2)
            };
            Grid.SetRow(thumbnail, 0);
            
            Grid InnerGrid = new Grid()
            {
                Margin = new Thickness(0,2,2,2)
            }; // Create the inner grid for styling
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            InnerGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(InnerGrid, 1);
            
            if (alternate) // Alternate the background color for readability
            {
                InnerGrid.Background = new SolidColorBrush(Colors.Gray);
                alternate = false;
            }
            else
            {
                InnerGrid.Background = new SolidColorBrush(Colors.DarkGray);
                alternate = true;
            }
            
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
            
            // Add the elements to the inner grid
            InnerGrid.Children.Add(Name);
            InnerGrid.Children.Add(Author);
            InnerGrid.Children.Add(Description);
            InnerGrid.Children.Add(Version);

            // Add the details and thumbnail to the outer grid row
            OuterGrid.Children.Add(thumbnail);
            OuterGrid.Children.Add(InnerGrid);
            
            // Add the outer grid to the stack panel
            stackPanel.Children.Add(OuterGrid);
        }
        scrollViewer.Content = stackPanel;
    }

    void onModClick(object? sender, RoutedEventArgs e)
    {
        Grid mod = (Grid)sender;
        Grid InnerGrid = (Grid)mod.Children[1];
        Image thumbnail = (Image)mod.Children[0];
        TextBlock name = (TextBlock)InnerGrid.Children[0];
        
        StackPanel stackPanel = (StackPanel)mod.Parent;
        foreach (Grid modinfo in stackPanel.Children) // Loop through the mods
        {
            modinfo.Background = new SolidColorBrush(Colors.Transparent); // Reset the background color
        }
        
        ScrollViewer scrollViewer = (ScrollViewer)stackPanel.Parent;
        Grid windowGrid = (Grid)scrollViewer.Parent;

        Grid modInfoGrid = windowGrid.FindControl<Grid>("modInfo");
        modInfoGrid.RowDefinitions.Clear();
        modInfoGrid.RowDefinitions.Add(new RowDefinition());
        modInfoGrid.RowDefinitions.Add(new RowDefinition());
        modInfoGrid.Children.Clear(); // Clear the mod info grid
        
        Image infoThumbnail = new Image()
        {
            Source = thumbnail.Source,
            Margin = new Thickness(2,2,0,2),
            Name = mod.Name
        };
        
        TextBlock modInfo = new TextBlock()
        {
            Text = ($"{((TextBlock)InnerGrid.Children[0]).Text} \n" +
                    $"By {((TextBlock)InnerGrid.Children[1]).Text} \n \n" +
                    $"{((TextBlock)InnerGrid.Children[2]).Text} \n \n" +
                    $"{((TextBlock)InnerGrid.Children[3]).Text}"),
            Margin = new Thickness(1),
            TextWrapping = TextWrapping.Wrap
        };
        
        Grid.SetRow(thumbnail, 0);
        Grid.SetRow(modInfo, 1);
        
        modInfoGrid.Children.Add(infoThumbnail);
        modInfoGrid.Children.Add(modInfo);
        
        windowGrid.FindControl<Button>("btnInstall").IsEnabled = true;

        mod.Background = new SolidColorBrush(Colors.LimeGreen); // Change the background color of the mod
    }
}