using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMAPIModManager;

public class CurseForgeAPI
{
    private static readonly HttpClient client = new HttpClient();

    public class Mod : CurseForgeAPI
    {
        private int id { get; set; }
        private string curseId { get; set; }
        private string name { get; set; }
        private string author { get; set; }
        private string description { get; set; }
        private string version { get; set; }
        
        public Mod(int id, string curseId, string name, string author, string description, string version)
        {
            this.id = id;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
        }
        
        public Mod(string curseId, string name, string author, string description, string version)
        {
            this.id = 0;
            this.curseId = curseId;
            this.name = name;
            this.author = author;
            this.description = description;
            this.version = version;
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
        string responseBody = await SendRequest("v1/mods/search?gameId=669&sortField=" + sort);
        JsonElement data = JsonDocument.Parse(responseBody).RootElement.GetProperty("data");
        
        foreach (JsonElement mod in data.EnumerateArray())
        {
            string name = mod.GetProperty("name").GetString();
            string author = mod.GetProperty("authors")[0].GetProperty("name").GetString();
            string description = mod.GetProperty("summary").GetString();
            string version = "1.0.0";
            string curseId = mod.GetProperty("id").GetInt32().ToString();
            mods.Add(new Mod(curseId, name, author, description, version));
        }
        return mods;
    }
}