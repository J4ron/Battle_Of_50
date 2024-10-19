using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Battle_Of_50.SlashCommandHandler.Handler;

public class SendRulesEmbed : InteractionModuleBase<SocketInteractionContext>
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [SlashCommand("sendembed", "Sendet zwei Embeds in verschiedene Channels.")]
    public async Task SendEmbed()
    {
  
        EmbedBuilder battleEmbed = new()
        {
            Title = "Battle of 50",
            Description = "🌍 **Battle of 50** ist ein spannendes PvP-Minecraft-Projekt, in dem 50 Spieler, 25 Teams zu je 2 Spielern gegeneinander antreten. " +
                          "Ziel ist es, als letztes Team zu überleben! Die Map (2000x2000 Blöcke) ist survival-freundlich. " +
                          "Erze wie Eisen und Gold werden automatisch geschmolzen, der Nether ist deaktiviert. Alle 30 Minuten erscheinen " +
                          "Loot-Drops mit seltenen Items wie Netherrite.\n\n" +
                          "⏳ Das Spiel läuft täglich für begrenzte Zeit (z.B. 18–20 Uhr). Offline-Spieler erscheinen als Zombies, die geplündert werden können. " +
                          "Die Map schrumpft stetig, was zu intensiven Kämpfen führt.\n\n" +
                          "⚡ **Zufällige Events** sorgen für zusätzliche Herausforderungen, darunter:\n" +
                          "💧 - Toxischer Regen\n" +
                          "🌊 - Monsun\n" +
                          "🌩️ - Schweres Unwetter\n" +
                          "🌬️ - Die Luft wird Knapp\n" +
                          "🌕 - Blutmond\n\n" +
                          "👑 Nutze Ressourcen strategisch und verteidige dein Team, um am Ende als einziges Team zu überleben!"
        };
        
        battleEmbed.WithThumbnailUrl("https://cdn.discordapp.com/attachments/1031183707651964979/1296054938513969206/konzept.png?ex=6710e4b8&is=670f9338&hm=47b26d9ad8de64cc1f148b46e8bf4bcc4a7ce74bc92bc41f589089ffcf01551d&");
        battleEmbed.WithImageUrl("https://cdn.discordapp.com/attachments/1031183707651964979/1294976538411798629/Title.png?ex=67104422&is=670ef2a2&hm=5429ac0e20ea56237475100cf80a2209e4a89d07a763970a1e592f111f638a8f&"); 
        battleEmbed.WithColor(Color.Blue);
        
        EmbedBuilder rulesEmbed = new()
        {
            Title = "Battle of 50",
            Description = "📜 **Regeln**\n" +
                          "1. Kein Teaming mit anderen Teams.\n" +
                          "2. Keine Hacks oder unerlaubte Mods.\n" +
                          "3. Keine Beleidigungen oder toxic Verhalten.\n" +
                          "4. Offline-Spieler können als Zombies getötet und geplündert werden.\n" +
                          "5. Loot-Drops dürfen von jedem Team gesammelt werden, seid vorsichtig!\n" +
                          "6. Die Organisatoren besitzen das **Hausrecht** und dürfen die Regeln jederzeit ändern sowie nach eigenem Ermessen Spieler bannen oder kicken.\n\n" +
                          "⚠️ Regelverletzungen führen zur Disqualifikation."
        };
        rulesEmbed.WithThumbnailUrl(
            "https://cdn.discordapp.com/attachments/1031183707651964979/1296054938182615060/regeln.png?ex=6710e4b8&is=670f9338&hm=d4a06f1bdd4d6d5ca0ca5ac8b90aa451ef39a0409fc9b8c9c3efab718cd12c92&");

        rulesEmbed.WithColor(Color.Blue);
        
        var battleChannel = Context.Guild.GetTextChannel(1295768002029158470);
        var rulesChannel = Context.Guild.GetTextChannel(1295767962258772060); 
        
        if (battleChannel != null)
        {
            await battleChannel.SendMessageAsync(embed: battleEmbed.Build());
        }
        
        if (rulesChannel != null)
        {
            await rulesChannel.SendMessageAsync(embed: rulesEmbed.Build());
        }
    }
}
