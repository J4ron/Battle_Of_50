using Discord.Interactions;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Register
{
    public class SlashCommandRegister
    {
        private readonly InteractionService _interactionService;

        public SlashCommandRegister(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public async Task RegisterCommandsAsync()
        {
            await _interactionService.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), null);
        }
    }
}