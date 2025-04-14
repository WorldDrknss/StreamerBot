using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        // Get variables
        CPH.TryGetArg("user", out string user);
        CPH.TryGetArg("commandSource", out string commandSource);

        // List of lurk messages with a placeholder for the username
        string[] lurkList = {
            "@{0} is lurking like a ninja… If you hear a thud, I failed."
            "@{0} has gone into stealth mode. Snacks may be involved."
            "@{0} is here, but also not here. Quantum lurking activated."
            "@{0} is lurking… probably watching with one eye while pretending to be productive."
            "@{0} is now in AFK mode. Any signs of life are purely coincidental."
            "@{0} has vanished into the digital void but may return for snacks or drama."
            "@{0} is lurking… but let's be real, I'm just avoiding responsibilities."
            "@{0} is here in spirit… and possibly in the fridge."
            "@{0} has engaged maximum lurk. Do not disturb, unless memes are involved."
            "@{0} is lurking… and totally not just watching cat videos on another tab."
        };

        // Pull a random lurk from the list and format it with the username
        string randomLurk = string.Format(lurkList[new Random().Next(lurkList.Length)], user);

        // Send the message to the correct platform
        switch (commandSource)
        {
            case "twitch":
                CPH.SendAction(randomLurk);
                break;
            case "youtube":
                CPH.SendYouTubeMessage(randomLurk);
                break;
            case "trovo":
                CPH.SendTrovoMessage(randomLurk);
                break;
        }
        return true;
    }
}