using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordManagementBot;

class UserAutoRoles
{
    public async Task UserJoinedAsync(SocketGuildUser user)
    {

        var json = File.ReadAllText("C:\\Users\\Jaron\\RiderProjects\\Battle Of 50\\Battle Of 50\\Data\\roles.json");
        var roles = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            
        foreach (var roleEntry in roles)
        {
            var roleName = roleEntry.Key; 
            var roleIdString = roleEntry.Value; 
                
            if (!string.Equals(roleIdString, "string", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(roleIdString, out var roleId))
            {
                var role = user.Guild.GetRole(roleId); 
                if (role != null)
                {
                    await user.AddRoleAsync(role);
                }
            }
        }
    }
}