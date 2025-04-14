using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        // Get variables
        string discordMessage = CPH.GetGlobalVar<string>("discordMessage",true);
        string discordURI = CPH.GetGlobalVar<string>("discordURI",true);
        CPH.TryGetArg("commandSource", out string commandSource);

        // Send the message to the correct platform
        switch (commandSource)
        {
            case "twitch":
                CPH.SendAction($"{discordMessage} {discordURI}");
                break;
            case "youtube":
                CPH.SendYouTubeMessage($"{discordMessage} {discordURI}");
                break;
            case "trovo":
                CPH.SendTrovoMessage($"{discordMessage} {discordURI}");
                break;
        }
        return true;
    }
}