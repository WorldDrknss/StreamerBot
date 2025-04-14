using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline
{
    // You can set the timeout to any length you need
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

    public void Init()
    {
        // Ensure we are working with a clean slate
        _httpClient.DefaultRequestHeaders.Clear();
    }

    public void Dispose()
    {
        // Free up allocations
        _httpClient.Dispose();
    }

    public bool Execute()
    {
        // Get First Input from Chat
        CPH.TryGetArg("input0", out string input0);

        // Determine Chat Source
        CPH.TryGetArg("commandSource", out string commandSource);

        // Switch Chat Sources
        switch (commandSource)
        {
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

    private void TwitchCall(string userName)
    {
        // Check if Twitch User Exists
        var targetUserInfo = CPH.TwitchGetUserInfoByLogin(userName);;

        if (targetUserInfo != null)
        {
            DateTime startDate = DateTime.UtcNow.AddDays(-7);
            DateTime endDate = DateTime.UtcNow;

            // Get User Login
            string userLogin = targetUserInfo.UserLogin;
            var clips = CPH.GetClipsForUser(userLogin);

            if (clips != null && clips.Count > 0)
            {
                Random rand = new Random();
                var randomClip = clips[rand.Next(clips.Count)];

                string directClipUrl = GetMp4Clip(randomClip.Id);
                PlayClipInOBS(directClipUrl, userName);
            }
        }
    }

    private string GetMp4Clip(string clipId)
    {
        // Get Auth Data From Streamer.Bot
        string token = CPH.TwitchOAuthToken;
        string clientId = CPH.TwitchClientId;
        string apiUrl = $"https://api.twitch.tv/helix/clips?id={clipId}";

        // Clear our old headers
        _httpClient.DefaultRequestHeaders.Clear();

        // Now set your Twitch API authorization
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        _httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);

        // Send Request
        HttpResponseMessage response = _httpClient.GetAsync(apiUrl).GetAwaiter().GetResult(); // Synchronous wait
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); // Synchronous wait
            var clipData = JObject.Parse(jsonResponse);
            CPH.LogInfo($"{clipData}");
            string clipUrl = clipData["data"][0]["url"].ToString();
            return clipUrl;
        }
        else
        {
            string error = $"Error fetching clip: {response.StatusCode}. Response: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}";
            CPH.LogInfo(error);
            throw new Exception(error);
        }
    }

    private void PlayClipInOBS(string clipUrl, string userName)
    {

        CPH.LogInfo($"Clip URL {clipUrl}");
        CPH.LogInfo($"User is {userName}");

        string scene = "ClipScene";
        string source = "ClipBrowser";
        string gdi = "ClipText";
        int clipDuration = 30;

        CPH.ObsSetGdiText(scene, gdi, userName);
        CPH.ObsSetBrowserSource(scene, source, clipUrl);

        CPH.ObsSetSourceVisibility(scene, source, true);
        CPH.ObsSetSourceVisibility(scene, gdi, true);
        CPH.LogInfo("Playing Twitch clip in OBS...");

        CPH.Wait(clipDuration * 1000); // Wait for clip duration (in milliseconds)

        CPH.ObsSetSourceVisibility(scene, source, false);
        CPH.ObsSetSourceVisibility(scene, gdi, false);
        CPH.LogInfo("Hiding clip scene in OBS.");
    }
}