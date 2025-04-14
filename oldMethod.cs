using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline {
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

    public void Init() {
        _httpClient.DefaultRequestHeaders.Clear();
    }

    public void Dispose() {
        _httpClient.Dispose();
    }

    public bool Execute() {
        CPH.TryGetArg("input0", out string input0);
        CPH.TryGetArg("commandSource", out string commandSource);

        switch (commandSource) {
            case "twitch":
                TwitchCall(input0);
                break;
            case "youtube":
                // CPH.SendYouTubeMessage(randomLurk);
                break;
            case "trovo":
                // CPH.SendTrovoMessage(randomLurk);
                break;
        }

        return true;
    }

    private async void TwitchCall(string username) {
        var userInfo = CPH.TwitchGetUserInfoByLogin(username);
        if (userInfo != null) {
            string userLogin = userInfo.UserLogin;
            var clips = CPH.GetClipsForUser(userLogin, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            if (clips != null && clips.Count > 0) {
                Random rand = new Random();
                var randomClip = clips[rand.Next(clips.Count)];

                CPH.LogInfo($"Random Clip: {randomClip.Url} - {randomClip.Title}");

                // Fetch the direct clip URL using Twitch API
                string directClipUrl = await GetClipUrlAsync(randomClip.Id);
                CPH.LogInfo($"Direct Clip URL: {directClipUrl}");

                // Play the clip in OBS
                PlayClipInOBS(directClipUrl, username);
            } else {
                CPH.LogInfo($"No clips found for user: {username}");
            }
        } else {
            CPH.LogInfo($"User {username} not found.");
        }
    }

    private async Task<string> GetClipUrlAsync(string clipId) {
        string accessToken = CPH.TwitchOAuthToken;
        string clientId = CPH.TwitchClientId;
        string url = $"https://api.twitch.tv/helix/clips?id={clipId}";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.GetAsync(url).GetAwaiter().GetResult();
        if (response.IsSuccessStatusCode) {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var clipData = JObject.Parse(jsonResponse);
            string clipUrl = clipData["data"][0]["url"].ToString();
            return clipUrl;
        } else {
            throw new Exception($"Error fetching clip: {response.StatusCode}");
        }
    }

    private void PlayClipInOBS(string clipUrl, string username) {
        string scene = "ClipScene";   
        string source = "ClipBrowser"; 
        int clipDuration = 30;        

        CPH.ObsSetGdiText(scene, "ClipText", username);
        CPH.ObsSetBrowserSource(scene, source, clipUrl);

        CPH.ObsSetSourceVisibility(scene, source, true);
        CPH.ObsSetSourceVisibility(scene, "ClipText", true);
        CPH.LogInfo("Playing Twitch clip in OBS...");

        CPH.Wait(clipDuration * 1000); 

        CPH.ObsSetSourceVisibility(scene, source, false);
        CPH.ObsSetSourceVisibility(scene, "ClipText", false);
        CPH.LogInfo("Hiding clip scene in OBS.");
    }
}
