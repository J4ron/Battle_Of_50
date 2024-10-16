using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Sockets;

namespace DiscordManagementBot.SlashCommandHandler.Handler;

    public class CreateTeamHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong CategoryId = 1294345986210271333;
        private const ulong GuildId = 1293570287228686459;

        [SlashCommand("teamcreate", "Erstelle ein neues Team")]
        public async Task TeamCreateAsync()
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Administrator)
            {
                await RespondAsync("Du hast keine berechtigung für diesen Command.");
                return;
            }
            
            var createModalButton = new ButtonBuilder()
            {
                Label = "Team erstellen",
                CustomId = "create_modal",
                Style = ButtonStyle.Success
            };

            var component = new ComponentBuilder()
                .WithButton(createModalButton);

            var infoembed = new EmbedBuilder()
            {
                Title = "Erstelle dein Team für Battle Of 50",
                Description = "Mach dir vorher Gedanken um folgende Sachen: \n - Team Name \n - Team Kürzel \n - Team Farbe (Hex) \n - Dein Team Partner's Discord Namen",
                Color = Color.Orange
            }.Build();

            await RespondAsync(embed: infoembed, components: component.Build());
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
            string teamPartnerName = modal.TeamPartner;
            
            var existingRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));

            if (existingRole != null)
            {
                await RespondAsync($"Eine Team mit dem Namen '{teamName}' existiert bereits.", ephemeral: true);
                return;
            }

            if (!Regex.IsMatch(teamColor, @"^#[0-9A-Fa-f]{6}$"))
            {
                await RespondAsync("Bitte gib eine gültige Hex-Farbe im Format #RRGGBB ein.", ephemeral: true);
                return;
            }

            var teamPartner = Context.Guild.Users.FirstOrDefault(u => u.Username == teamPartnerName);

            if (teamPartner == null)
            {
                await RespondAsync($"Der Benutzer '{teamPartnerName}' wurde nicht gefunden. Bitte überprüfe den Namen und versuche es erneut.", ephemeral: true);
                return;
            }

            await RespondAsync($"Eine Einladung zu dem team {@teamName} wurde an den Spieler {teamPartnerName} gesendet!", ephemeral: true);
            
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
            await voiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, denyPermissions);
            
            var teamembed = new EmbedBuilder()
            {
                Title = $"Team {teamName}",
                Description = $"Weitere Informationen & Commands\n\n - /teamleave {teamName}\n\n -  /teaminvite USER {teamName}",
                Color = Color.Orange
            }.Build();
            
            await teamTextChannel.SendMessageAsync(embed: teamembed);
            
            var creator = Context.User as SocketGuildUser;
            await creator.AddRoleAsync(createdRole);
            
            
            var dmEmbed = new EmbedBuilder()
            {
                Title = "Team Einladungsanfrage",
                Description = $"Du wurdest von {creator.Mention} eingeladen, dem Team **{teamName}** beizutreten.\n\n**Team Kürzel:** {teamAbbr}\n**Team Farbe:** {teamColor}\n\nMöchtest du das Team annehmen oder ablehnen?",
                Color = Color.Green
            }.Build();

            var dmComponent = new ComponentBuilder()
                .WithButton("Annehmen", $"team_accept:{createdRole}", ButtonStyle.Success)
                .WithButton("Ablehnen", $"team_decline", ButtonStyle.Danger)
                .Build();
            
            try
            {
                await teamPartner.SendMessageAsync(embed: dmEmbed, components: dmComponent);
            }
            catch
            {
                await RespondAsync($"Team '{teamName}' wurde erstellt, aber die DM an {teamPartner.Mention} konnte nicht gesendet werden.", ephemeral: true);
            }
        }

        [ComponentInteraction("team_accept:*")]
        public async Task AcceptTeamInvite(string createdRoleName)
        {
            var guild = Context.Client.GetGuild(GuildId);

            if (guild == null)
            {
                await RespondAsync("Die Guild konnte nicht gefunden werden.", ephemeral: true);
                return;
            }

            var guildUser = guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);

            if (guildUser == null)
            {
                await RespondAsync("Der Benutzer konnte in der Guild nicht gefunden werden.", ephemeral: true);
                return;
            }
            
            var role = guild.Roles.FirstOrDefault(r => r.Name.Equals(createdRoleName, StringComparison.OrdinalIgnoreCase));

            if (role != null)
            {
                await guildUser.AddRoleAsync(role);
                await RespondAsync($"Du hast die Rolle **{role.Name}** erhalten!", ephemeral: true);
                
            }
            else
            {
                await RespondAsync("Die Rolle konnte nicht gefunden werden.", ephemeral: true);
            }
        }



        [ComponentInteraction("team_decline")]
        public async Task DeclineTeamInvite()
        {
            await RespondAsync("Du hast die Team-Einladung abgelehnt.", ephemeral: true);
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

            [InputLabel("Teampartner (Discord Name)")]
            [ModalTextInput("team_partner", TextInputStyle.Short, placeholder: "Gib den Discord Namen deines Partners ein", minLength: 4)]
            public string TeamPartner { get; set; }
        }
    }

