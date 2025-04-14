using System;

public class CPHInline
{
    public bool Execute()
    {
        CPH.TryGetArg("input0", out string input0);
        CPH.TryGetArg("commandSource", out string commandSource);

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

    private void TwitchCall(string username)
    {
        var userInfo = CPH.TwitchGetUserInfoByLogin(username);

        if (userInfo != null)
        {
            string userLogin = userInfo.UserLogin;  

            // Define date range for clips
            DateTime startDate = DateTime.UtcNow.AddDays(-7);
            DateTime endDate = DateTime.UtcNow;

            // Fetch clips using username
            var clips = CPH.GetClipsForUser(userLogin, startDate, endDate);

            if (clips != null && clips.Count > 0)
            {
                Random rand = new Random();
                var randomClip = clips[rand.Next(clips.Count)];  // Get a random clip

                CPH.LogInfo($"Random Clip: {randomClip.EmbedUrl} - {randomClip.Title}");

                // Send clip to OBS
                PlayClipInOBS(randomClip.Url, username);
            }
            else
            {
                CPH.LogInfo($"No clips found for user: {username}");
            }
        }
        else
        {
            CPH.LogInfo($"User {username} not found.");
        }
    }

    private void PlayClipInOBS(string clipUrl, string username)
    {
        string scene = "ClipScene";   // Change to your actual scene name
        string source = "ClipBrowser"; // Change to your actual browser source name
        int clipDuration = 30;        // Adjust the expected clip duration (in seconds)
        
        CPH.ObsSetGdiText(scene, "ClipText", username);

        // Set the clip URL in the OBS browser source
        CPH.ObsSetBrowserSource(scene, source, clipUrl);

        // Show the scene
        CPH.ObsSetSourceVisibility(scene, source, true);
        CPH.ObsSetSourceVisibility(scene, "ClipText", true);
        CPH.LogInfo("Playing Twitch clip in OBS...");

        // Wait for the clip duration
        CPH.Wait(clipDuration * 1000); // Convert seconds to milliseconds

        // Hide the scene after playback
        CPH.ObsSetSourceVisibility(scene, source, false);
        CPH.ObsSetSourceVisibility(scene, "ClipText", false);
        CPH.LogInfo("Hiding clip scene in OBS.");
    }
}
