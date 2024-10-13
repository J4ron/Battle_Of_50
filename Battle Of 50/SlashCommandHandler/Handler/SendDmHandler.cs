using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordManagementBot.SlashCommandHandler.Commands
{
    public class SendDmCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong GuildId = 1293570287228686459; // Deine Guild-ID
        private ulong _roleIdToGive; // Rolle, die zugewiesen werden soll

        [SlashCommand("senddm", "Sendet eine DM mit Buttons an einen Benutzer.")]
        public async Task SendDmAsync(SocketUser user, SocketRole roleToGive)
        {
            if (user is not SocketGuildUser targetUser)
            {
                await RespondAsync("Bitte wähle einen gültigen Benutzer aus.", ephemeral: true);
                return;
            }

            // Überprüfe, ob die Rolle gültig ist
            if (roleToGive == null)
            {
                await RespondAsync("Bitte wähle eine gültige Rolle aus.", ephemeral: true);
                return;
            }

            _roleIdToGive = roleToGive.Id; // Speichere die Rolle ID in einem Feld

            // Buttons erstellen
            var confirmButton = new ButtonBuilder()
                .WithLabel("Klicken um zu bestätigen")
                .WithCustomId("confirm_button")
                .WithStyle(ButtonStyle.Success);

            var denyButton = new ButtonBuilder()
                .WithLabel("Ablehnen")
                .WithCustomId("deny_button")
                .WithStyle(ButtonStyle.Danger);

            var component = new ComponentBuilder()
                .WithButton(confirmButton)
                .WithButton(denyButton);

            try
            {
                // DM an den Benutzer senden
                await targetUser.SendMessageAsync("Hallo! Drücke einen Button, um zu bestätigen oder abzulehnen.", components: component.Build());

                // Bestätige den Command
                await RespondAsync($"DM wurde erfolgreich an {targetUser.Username} gesendet. mit der RollenId {_roleIdToGive}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Fehler beim Senden der DM: {ex.Message}", ephemeral: true);
            }
        }

        // Button-Interaktion für Bestätigen
        [ComponentInteraction("confirm_button")]
        public async Task ConfirmButtonAsync()
        {
            var guild = Context.Client.GetGuild(GuildId); // Guild anhand der ID abrufen

            if (guild == null)
            {
                await RespondAsync("Die Guild konnte nicht gefunden werden.", ephemeral: true);
                return;
            }

            // Hole den Benutzer in der Guild
            var guildUser = guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);

            if (guildUser == null)
            {
                await RespondAsync("Der Benutzer konnte in der Guild nicht gefunden werden.", ephemeral: true);
                return;
            }

            // Hole die Rolle basierend auf der ID
            var role = guild.Roles.FirstOrDefault(r => r.Id == _roleIdToGive);
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

        // Button-Interaktion für Ablehnen
        [ComponentInteraction("deny_button")]
        public async Task DenyButtonAsync()
        {
            // Deaktiviere den Ablehnen-Button
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
