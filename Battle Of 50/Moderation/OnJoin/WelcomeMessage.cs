using Discord;
using Discord.WebSocket;

namespace DiscordManagementBot;

class WelcomeMessage
{
    public async Task SendWelcomeMessageAsync(SocketGuildUser user)
    {
        var guild = user.Guild;
        var channel = guild.DefaultChannel;
        if (channel == null) return;
        var allMembers = guild.MemberCount;

        var embed = new EmbedBuilder()
            .WithTitle($"Willkommen, {user.Username}!")
            .WithDescription("Viel Spaß bei Battle Of 50")
            .WithDescription("Du bist der " + allMembers + ". Teilnehmer!")
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetGuildAvatarUrl())
            .WithFooter("powerd by undefinedstudios")
            .WithCurrentTimestamp()
            .WithColor(Color.Green)
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }
}