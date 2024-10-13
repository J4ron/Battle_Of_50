using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordManagementBot.SlashCommandHandler.Commands
{
    public class SendDmCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong GuildId = 1293570287228686459;
        private ulong _roleIdToGive; 
        
        [SlashCommand("giveroleto", "Sendet eine DM mit Buttons an einen Benutzer.")]
        public async Task SendDmAsync(SocketUser user, SocketRole roleToGive)
        {
            if (!((SocketGuildUser)Context.User).GuildPermissions.Administrator)
            {
                await RespondAsync("Du hast keine berechtigung für diesen Command.");
                return;
            }
            
            if (user is not SocketGuildUser targetUser)
            {
                await RespondAsync("Bitte wähle einen gültigen Benutzer aus.", ephemeral: true);
                return;
            }
            
            if (roleToGive == null)
            {
                await RespondAsync("Bitte wähle eine gültige Rolle aus.", ephemeral: true);
                return;
            }

            
            _roleIdToGive = roleToGive.Id;
            Console.WriteLine(roleToGive.Id);
            
            var confirmButton = new ButtonBuilder()
                .WithLabel("Klicken um zu bestätigen")
                .WithCustomId($"confirm_button:{_roleIdToGive}") 
                .WithStyle(ButtonStyle.Success);
            
            Console.WriteLine($"CustomId: confirm_button:{_roleIdToGive}");


            var denyButton = new ButtonBuilder()
                .WithLabel("Ablehnen")
                .WithCustomId("deny_button")
                .WithStyle(ButtonStyle.Danger);

            var component = new ComponentBuilder()
                .WithButton(confirmButton)
                .WithButton(denyButton);

            try
            {

                await targetUser.SendMessageAsync("Hallo! Drücke einen Button, um zu bestätigen oder abzulehnen.", components: component.Build());

                await RespondAsync($"DM wurde erfolgreich an {targetUser.Username} gesendet. mit der RollenId {_roleIdToGive}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Fehler beim Senden der DM: {ex.Message}", ephemeral: true);
            }
        }
        
        [ComponentInteraction("confirm_button:*")]
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
            
            if (ulong.TryParse(roleId, out ulong parsedRoleId))
            {
                var role = guild.Roles.FirstOrDefault(r => r.Id == parsedRoleId);
                Console.WriteLine(parsedRoleId);
                if (role != null)
                {
                    await guildUser.AddRoleAsync(role);
                    await RespondAsync($"Du hast die Rolle {role.Name} erhalten!", ephemeral: true);
                }
                else
                {
                    await RespondAsync("Die Rolle konnte nicht gefunden werden.", ephemeral: true);
                }
            }
            else
            {
                await RespondAsync("Ungültige Rollen-ID.", ephemeral: true);
            }
        }
        
        [ComponentInteraction("deny_button")]
        public async Task DenyButtonAsync()
        {
            var disabledButton = new ButtonBuilder()
                .WithLabel("Ablehnung bestätigt")
                .WithCustomId("deny_button")
                .WithStyle(ButtonStyle.Danger)
                .WithDisabled(true);

            var component = new ComponentBuilder()
                .WithButton(disabledButton);

            await RespondAsync("Du hast die Ablehnung bestätigt.", components: component.Build(), ephemeral: true);
        }
    }
}
