using Discord.WebSocket;

namespace DiscordManagementBot.Moderation.MemberCount;

public class MemberCount
{
    private readonly ulong _currentMemberCountChannelId = 1294249662341054474;
    private int _maxMembers = 0; 

    public async Task UpdateMemberCountAsync(SocketGuild guild)
    {
        int currentMemberCount = guild.MemberCount;
        
        
        var currentChannel = guild.GetVoiceChannel(_currentMemberCountChannelId);

        if (currentChannel != null)
        {
            await currentChannel.ModifyAsync(prop => prop.Name = $"Spieler: {currentMemberCount}");
        }
        
    }
}