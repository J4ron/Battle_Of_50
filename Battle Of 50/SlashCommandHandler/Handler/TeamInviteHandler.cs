using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Commands
{
    public class SendTeamInviteCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong GuildId = 1293570287228686459; 
        private const int MaxTeamMembers = 2; 
        
        private static readonly ConcurrentDictionary<ulong, (ulong RoleId, DateTime ExpirationTime, bool IsDeclined)> invitationStatuses = new();

        [SlashCommand("teaminvite", "Sendet eine DM mit Buttons an einen Benutzer.")]
        public async Task SendDmAsync(SocketUser user, string teamName)
        {
            var guild = Context.Client.GetGuild(GuildId);
            if (guild == null)
            {
                await RespondAsync("Die Guild konnte nicht gefunden werden.", ephemeral: true);
                return;
            }
            
            var teamRole = guild.Roles.FirstOrDefault(r => string.Equals(r.Name, teamName, StringComparison.OrdinalIgnoreCase));
            if (teamRole == null)
            {
                await RespondAsync("Es gibt keine Rolle mit diesem Teamnamen.", ephemeral: true);
                return;
            }
            
            var executingUser = Context.User as SocketGuildUser;
            if (executingUser == null || !executingUser.Roles.Any(r => r.Id == teamRole.Id))
            {
                await RespondAsync("Du bist kein Mitglied dieses Teams und kannst diesen Befehl nicht ausführen.", ephemeral: true);
                return;
            }
            
            var currentTeamMembers = guild.Users.Count(u => u.Roles.Any(r => r.Id == teamRole.Id));
            if (currentTeamMembers >= MaxTeamMembers)
            {
                await RespondAsync("Das Team hat bereits die maximale Anzahl an Mitgliedern erreicht.", ephemeral: true);
                return;
            }

            if (user is not SocketGuildUser targetUser)
            {
                await RespondAsync("Bitte wähle einen gültigen Benutzer aus.", ephemeral: true);
                return;
            }
            
            invitationStatuses[targetUser.Id] = (teamRole.Id, DateTime.UtcNow.AddMinutes(5), false);

            var confirmButton = new ButtonBuilder()
                .WithLabel("Klicken um zu bestätigen")
                .WithCustomId($"confirm:{teamRole.Id}") 
                .WithStyle(ButtonStyle.Success);

            var denyButton = new ButtonBuilder()
                .WithLabel("Ablehnen")
                .WithCustomId("deny")
                .WithStyle(ButtonStyle.Danger);

            var component = new ComponentBuilder()
                .WithButton(confirmButton)
                .WithButton(denyButton);

            var embed = new EmbedBuilder()
            {
                Title = "Team Einladungsanfrage",
                Description = $"Du wurdest für das Team **{teamName}** eingeladen.\n\nDrücke einen Button, um zu bestätigen oder abzulehnen.",
                Color = Color.Green
            }.Build();

            try
            {
                await targetUser.SendMessageAsync(embed: embed, components: component.Build());
                await RespondAsync($"DM wurde erfolgreich an {targetUser.Username} gesendet. Team: {teamName}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Fehler beim Senden der DM: {ex.Message}", ephemeral: true);
            }
        }

        [ComponentInteraction("confirm:*")]
        public async Task ConfirmButtonAsync(string roleId)
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
            
            if (invitationStatuses.TryGetValue(Context.User.Id, out var invitation))
            {
                if (invitation.IsDeclined)
                {
                    await RespondAsync("Du hast die Einladung bereits abgelehnt und kannst sie nicht erneut annehmen.", ephemeral: true);
                    return;
                }
                
                if (DateTime.UtcNow > invitation.ExpirationTime)
                {
                    await RespondAsync("Die Einladung ist abgelaufen.", ephemeral: true);
                    return;
                }

                var currentTeamMembers = guild.Users.Count(u => u.Roles.Any(r => r.Id == invitation.RoleId));
                if (currentTeamMembers >= MaxTeamMembers)
                {
                    await RespondAsync("Das Team hat bereits die maximale Anzahl an Mitgliedern erreicht. Du kannst nicht beitreten.", ephemeral: true);
                    return;
                }

                var role = guild.Roles.FirstOrDefault(r => r.Id == invitation.RoleId);
                if (role != null)
                {
                    await guildUser.AddRoleAsync(role);
                    await RespondAsync($"Du hast das Team {role.Name} betreten!", ephemeral: true);
                    
                    invitationStatuses.TryRemove(Context.User.Id, out _);
                }
                else
                {
                    await RespondAsync("Die Rolle konnte nicht gefunden werden.", ephemeral: true);
                }
            }
            else
            {
                await RespondAsync("Du hast keine Einladung für dieses Team.", ephemeral: true);
            }
        }

        [ComponentInteraction("deny")]
        public async Task DenyButtonAsync()
        {
            if (invitationStatuses.TryGetValue(Context.User.Id, out var invitation))
            {
                invitationStatuses[Context.User.Id] = (invitation.RoleId, invitation.ExpirationTime, true);
            }

            var disabledButton = new ButtonBuilder()
                .WithLabel("Ablehnung bestätigt")
                .WithCustomId("deny")
                .WithStyle(ButtonStyle.Danger)
                .WithDisabled(true);

            var component = new ComponentBuilder()
                .WithButton(disabledButton);

            await RespondAsync("Du hast die Ablehnung bestätigt.", components: component.Build(), ephemeral: true);
        }
    }
}
