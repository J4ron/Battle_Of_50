using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Handler
{
    public class KickCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("kick", "Kicks a user from the server")]
        public async Task KickUser(SocketGuildUser user, string reason = "No reason provided")
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.KickMembers)
            {
                await RespondAsync("You do not have permission to use this command.");
                return;
            }

            await user.KickAsync(reason);
            await RespondAsync($"{user} has been kicked for: {reason}");
        }
    }
}