using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace DiscordManagementBot.SlashCommandHandler.Handler
{
    public class CreateTeamHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong CategoryId = 1294345986210271333;

        [RequireUserPermission(GuildPermission.Administrator)]
        [SlashCommand("teamcreate", "Erstelle ein neues Team")]
        public async Task TeamCreateAsync()
        {
            var createModalButton = new ButtonBuilder()
            {
                Label = "Team erstellen",
                CustomId = "create_modal",
                Style = ButtonStyle.Success
            };

            var component = new ComponentBuilder()
                .WithButton(createModalButton);

            var infoEmbed = new EmbedBuilder()
            {
                Title = "Erstelle dein Team für Battle Of 50",
                Description = "Mach dir vorher Gedanken um folgende Sachen: \n - Team Name \n - Team Kürzel \n - Team Farbe (Hex)",
                Color = Color.Orange
            }.Build();

            await RespondAsync(embed: infoEmbed, components: component.Build());
        }

        [ComponentInteraction("create_modal")]
        public async Task HandleButtonInput()
        {
            await RespondWithModalAsync<TeamModal>("team_modal");
        }

        [ModalInteraction("team_modal")]
        public async Task HandleModalInput(TeamModal modal)
        {
            string teamName = modal.TeamName;
            string teamAbbr = modal.TeamAbbreviation;
            string teamColor = modal.TeamColor;

            if (!Regex.IsMatch(teamColor, @"^#[0-9A-Fa-f]{6}$"))
            {
                await RespondAsync("Bitte gib eine gültige Hex-Farbe im Format #RRGGBB ein.", ephemeral: true);
                return;
            }
            
            var createdRole = await Context.Guild.CreateRoleAsync(
                teamName,
                isMentionable: true,
                isHoisted: true,
                color: new Color(Convert.ToUInt32(teamColor.TrimStart('#'), 16))
            );

            var teamTextChannel = await Context.Guild.CreateTextChannelAsync(
                $"{teamName} Text",
                properties =>
                {
                    properties.CategoryId = CategoryId;
                }
            );

            var voiceChannel = await Context.Guild.CreateVoiceChannelAsync(
                $"{teamName} Voice",
                properties =>
                {
                    properties.CategoryId = CategoryId;
                }
            );

            var overwritePermissions = new OverwritePermissions(
                viewChannel: PermValue.Allow,
                sendMessages: PermValue.Allow,
                readMessageHistory: PermValue.Allow,
                connect: PermValue.Allow, 
                speak: PermValue.Allow
            );

            await teamTextChannel.AddPermissionOverwriteAsync(createdRole, overwritePermissions);
            await voiceChannel.AddPermissionOverwriteAsync(createdRole, overwritePermissions);
            
            var denyPermissions = new OverwritePermissions(
                viewChannel: PermValue.Deny
            );

            await teamTextChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, denyPermissions);
            
            
            var commandEmbed = new EmbedBuilder()
            {
                Title = "Weitere Commands",
                Description = $"Hier sind einige nützliche Befehle, die du als Teamleiter verwenden kannst:\n" +
                              $"/leaveteam {teamName}\n" +
                              $"/teaminvite <player> {teamName}\n" +
                              $"/deleteteam {teamName}",
                Color = Color.Orange
            }.Build();
            
            await teamTextChannel.SendMessageAsync(embed: commandEmbed);

            await voiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, denyPermissions);
            
            var creator = Context.User as SocketGuildUser;
            await creator.AddRoleAsync(createdRole);
            
            var dmEmbed = new EmbedBuilder()
            {
                Title = "Team Einladungsanfrage",
                Description = $"Du wurdest von {creator.Mention} eingeladen, dem Team **{teamName}** beizutreten.\n\n**Team Kürzel:** {teamAbbr}\n**Team Farbe:** {teamColor}\n\nMöchtest du die Einladung annehmen oder ablehnen?",
                Color = Color.Green
            }.Build();

            var dmComponent = new ComponentBuilder()
                .WithButton("Annehmen", $"team_accept_{teamName}", ButtonStyle.Success)
                .WithButton("Ablehnen", $"team_decline_{teamName}", ButtonStyle.Danger)
                .Build();

            try
            {
                await RespondAsync($"Team '{teamName}' wurde erstellt.", ephemeral: true);
            }
            catch
            {
                //
            }
        }
        

        public class TeamModal : IModal
        {
            public string Title => "Erstelle dein Team";

            [InputLabel("Team Name")]
            [ModalTextInput("team_name", TextInputStyle.Short, placeholder: "Gib hier den Namen deines Teams ein", minLength: 4, maxLength: 20)]
            public string TeamName { get; set; }

            [InputLabel("Team Kürzel")]
            [ModalTextInput("team_abbreviation", TextInputStyle.Short, placeholder: "Max 4 Zeichen", maxLength: 4)]
            public string TeamAbbreviation { get; set; }

            [InputLabel("Team Farbe (Hex)")]
            [ModalTextInput("team_color", TextInputStyle.Short, placeholder: "#eb4034", minLength: 7, maxLength: 7)]
            public string TeamColor { get; set; }
            
        }
    }
}
