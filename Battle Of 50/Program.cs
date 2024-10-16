using BattleOf50.SlashCommandHandler.Register;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordManagementBot.Moderation.MemberCount;


namespace DiscordManagementBot
{
    internal class Program
    {
        private DiscordSocketClient? _client;
        private InteractionService? _interactionService;
        private IServiceProvider? _services;
        private readonly WelcomeMessage _welcomeMessage = new();
        private readonly MemberCount _memberCount = new();
        private readonly UserAutoRoles _userAutoRoles = new();
        private readonly ActiveContentFilter _activeContentFilter = new();

        public WelcomeMessage WelcomeMessage => _welcomeMessage;
        public UserAutoRoles UserAutoRoles => _userAutoRoles;
        public ActiveContentFilter ActiveContentFilter => _activeContentFilter;

        private static async Task Main(string[] args)
        {
            await new Program().RunBotAsync();
        }

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent |
                                 GatewayIntents.GuildMembers | GatewayIntents.GuildVoiceStates | GatewayIntents.All
            });

            _interactionService = new InteractionService(_client);

            ClientOnActionhandler();

            await _client.LoginAsync(TokenType.Bot, "MTI5MzU3MzI3NTI4ODMzODUwMg.GiGaM9.2AE2NV8sYdBsqzY_y03vUbRM4ttDc1efgj0ydM");
            await _client.StartAsync();

            var slashCommandRegister = new SlashCommandRegister(_interactionService);
            await slashCommandRegister.RegisterCommandsAsync();

            await Task.Delay(-1);
        }

        private void ClientOnActionhandler()
        {
            _client.Log += Log;
            _client.Ready += ReadyAsync;  
            _client.UserJoined += UserAutoRoles.UserJoinedAsync;
            _client.UserJoined += WelcomeMessage.SendWelcomeMessageAsync;
            _client.MessageReceived += ActiveContentFilter.MessageReceivedAsync;

            _client.UserJoined += async (SocketGuildUser user) =>
            {
                await _memberCount.UpdateMemberCountAsync(user.Guild);
            };

            _client.InteractionCreated += HandleInteractionAsync;
        }

        private async Task ReadyAsync()
        {
            
            await _interactionService.RegisterCommandsToGuildAsync(1293570287228686459);
            foreach (var guild in _client.Guilds)
            {
                Console.WriteLine($"Mitglieder in der Guild: {guild.Name}");
                
                Console.WriteLine("Rollen in der Guild:");
                foreach (var role in guild.Roles)
                {
                    Console.WriteLine($"- {role.Name} (ID: {role.Id})");
                }
                
                await guild.DownloadUsersAsync();
                
                foreach (var member in guild.Users)
                {
                    Console.WriteLine($"Mitglied: {member.Username}#{member.Discriminator}");
                }
            }
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling interaction: {ex.Message}");
            }
        }
    }
}