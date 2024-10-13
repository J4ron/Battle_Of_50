using Discord.Interactions;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Response
{
    public class CommandResponses
    {
        public static async Task RespondAsync(InteractionContext context, string message)
        {
            await context.Interaction.RespondAsync(message);
        }
    }
}