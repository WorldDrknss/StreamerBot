using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

public class CPHInline
{
    private static readonly HttpClient client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    
    public bool Execute()
    {
    	// Get variables
        CPH.TryGetArg("user", out string user);
        CPH.TryGetArg("rawInput", out string rawInput);
        
        // Attempt to change the game with exactly what the user typed
        var gameInfo = CPH.SetChannelGame(rawInput);
        
        // If it fails to update, check if we can find the game on IGDB
        if (gameInfo == null)
        {
            try
            {
                CPH.LogInfo("Attempting to get the IGDB name...");
                string IGDBGameName = GetIGDBGameName(rawInput).GetAwaiter().GetResult();
                gameInfo = CPH.SetChannelGame(IGDBGameName);
                
                // If we still don't get anything, notify the user
                if (gameInfo == null)
                {
                    CPH.SendMessage($"@{user}, failed to change the Twitch category. Check for the exact spelling and try again.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                CPH.LogInfo("Whoopsie Daisie");
                return false;
            }
        }

        return true;
    }
	
	// Check IGDB for exact game name
    private async Task<string> GetIGDBGameName(string keyword)
    {
        // Get Twitch credentials
        string accessToken = CPH.TwitchOAuthToken;
        string clientId = CPH.TwitchClientId;
        
        // Making call hehe
        var url = "https://api.igdb.com/v4/games";
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Client-ID", clientId);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        
        var requestBody = new StringContent($"fields name,url,cover.image_id; search \"{keyword}\";", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, requestBody);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonResponse = JArray.Parse(responseBody);
        
        JObject selectedGame = null;
        string IGDBGameName = null;
        
        foreach (var game in jsonResponse)
        {
            if (string.Equals(game["name"]?.ToString(), keyword, StringComparison.OrdinalIgnoreCase))
            {
                selectedGame = (JObject)game;
                break;
            }
        }

        if (selectedGame == null && jsonResponse.Count > 0)
        {
            // pick first result
            selectedGame = (JObject)jsonResponse[1];
        }

        if (selectedGame != null)
        {
            // pick game name of first result
            IGDBGameName = selectedGame["name"]?.ToString() ?? "Unknown Title";
        }
        else
        {
            CPH.LogInfo($"No game found for: '{keyword}.");
        }

        CPH.LogInfo("Returned IGDB Name: " + IGDBGameName);
        return IGDBGameName;
    }
}