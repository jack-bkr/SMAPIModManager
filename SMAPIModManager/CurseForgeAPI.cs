using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMAPIModManager;

public class CurseForgeAPI
{
    private static readonly HttpClient client = new HttpClient();
    
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
}