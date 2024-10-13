using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Handler
{
    public class BanCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ban", "Bans a user from the server")]
        public async Task BanUser(SocketGuildUser user, string reason = "No reason provided")
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.BanMembers)
            {
                await RespondAsync("You do not have permission to use this command.");
                return;
            }

            await Context.Guild.AddBanAsync(user, reason: reason);
            await RespondAsync($"{user} has been banned for: {reason}");
        }
    }
}