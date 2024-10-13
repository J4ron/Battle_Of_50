using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Battle_Of_50.SlashCommandHandler.Handler;
public class TeamLeaveHandler : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("teamleave", "Verlasse ein bestehendes Team")]
    public async Task TeamLeaveAsync(string teamName)
    {
        var guildUser = Context.User as SocketGuildUser;

        var teamRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(teamName, System.StringComparison.OrdinalIgnoreCase));

        if (teamRole == null)
        {
            await RespondAsync($"Es gibt kein Team mit dem Namen '{teamName}'.", ephemeral: true);
            return;
        }
        
        if (!guildUser.Roles.Contains(teamRole))
        {
            await RespondAsync($"Du bist kein Mitglied des Teams '{teamName}'.", ephemeral: true);
            return;
        }
        
        await guildUser.RemoveRoleAsync(teamRole);
        await RespondAsync($"Du hast das Team '{teamName}' erfolgreich verlassen.", ephemeral: true);
        
        Console.WriteLine($"{guildUser.Username} hat das Team '{teamName}' verlassen.");

        var teamTextChannel = Context.Guild.TextChannels.FirstOrDefault(c => c.Name.Equals($"{teamName.ToLower()}-text", System.StringComparison.OrdinalIgnoreCase));
        var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(c => c.Name.Equals($"{teamName} Voice", System.StringComparison.OrdinalIgnoreCase));

        await teamTextChannel.SendMessageAsync($"{guildUser} hat das Team {teamName} verlassen.");
        
        var remainingMembers = Context.Guild.Users.Where(u => u.Roles.Contains(teamRole)).ToList();
        
        Console.WriteLine($"Es verbleiben {remainingMembers.Count} Mitglieder im Team '{teamName}'.");

        if (!remainingMembers.Any())
        {
            await teamTextChannel?.DeleteAsync();
            await voiceChannel?.DeleteAsync();
            await teamRole.DeleteAsync();
            
            Console.WriteLine($"Das Team '{teamName}' wurde gelöscht, da keine Mitglieder mehr vorhanden sind.");
        }
    }
}
