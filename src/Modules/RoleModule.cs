using Discord;
using Discord.Interactions;

namespace IndYBot.Modules;

[Group("role", "Manage roles")]
public class RoleModule : InteractionModuleBase<SocketInteractionContext> 
{
   [SlashCommand("grouprole", "Create a role used for groups!")]
   public async Task CreateGroupRoleCommand(
         [Summary("name", "Set the name of the role!")] string name,
         [Summary("color", "Set the color of the role!")] string color)
   {
      var role = await Context.Guild.CreateRoleAsync(name, color: Color.Parse(color), isHoisted: false, isMentionable: true);
      var user = (IGuildUser) Context.User;

      await user.AddRoleAsync(role.Id);

      await RespondAsync($"Successfully created role: {name}", ephemeral: true);
   }

   [SlashCommand("add_user", "Add a user to a group!")]
   public async Task AddUserToGroup(
         [Summary("role", "The role to join!")] IRole role,
         [Summary("user", "The user to join!")] IUser user)
   {
      var guildUser = (user as IGuildUser); 
      if (guildUser == null)
      {
         await RespondAsync("Error at user handling!", ephemeral: true);
         return;
      }

      await guildUser.AddRoleAsync(role.Id);

      await RespondAsync($"Successfully add user {user} to role {role.Name}", ephemeral: true);
   }
}
