using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordManagementBot;

class ActiveContentFilter
{
    public async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message is not IUserMessage userMessage || message.Author.IsBot) return;

        var json = File.ReadAllText(
            "C:\\Users\\Jaron\\RiderProjects\\Battle Of 50\\Battle Of 50\\Data\\banned.json");
        var bannedWords = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        foreach (var bannedWord in bannedWords.Values)
        {
            if (!string.Equals(bannedWord, "string", StringComparison.OrdinalIgnoreCase) &&
                message.Content.Contains(bannedWord, StringComparison.OrdinalIgnoreCase))
            {
                await message.DeleteAsync();
                return;
            }
        }

        if (message.Content.Contains("http://") || message.Content.Contains("https://"))
        {
            await message.DeleteAsync();
        }
    }
}