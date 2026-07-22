using Discord;
using Discord.Interactions;
using IndYBot.Helpers;

namespace IndYBot.Modules;

[Group("role", "Manage roles")]
public class RoleModule : InteractionModuleBase<SocketInteractionContext> 
{
   [SlashCommand("grouprole", "Create a group role! (*name*_group)")]
   public async Task CreateGroupRoleCommand(
         [Summary("name", "Set the name of the role!")] string name,
         [Summary("color", "Set the color of the role!")] string color)
   {
      var roleName = name + "_group";

      var role = await Context.Guild.CreateRoleAsync(roleName, color: Color.Parse(color), isHoisted: false, isMentionable: true);
      var user = (IGuildUser) Context.User;

      await user.AddRoleAsync(role.Id);

      await RespondAsync($"Successfully created role: {name}", ephemeral: true);
   }

   [SlashCommand("list_groups", "List all groups a user is in!")]
   public async Task ListGroupsCommand(
         [Summary("user", "The user to join!")] IUser user)
   {
      await RespondAsync($"# Groups for user {user.Mention}\n");

      var guildUser = (IGuildUser) user;
      if (guildUser == null)
      {
         await RespondAsync("Error at user handling!", ephemeral: true);
         return;
      }

      var roles = guildUser.RoleIds;
      
      await MessageHelper.SendListMessageAsync(
            roles.ToList(),
            Context,
            roleId => {
               var role = Context.Guild.GetRole(roleId);

               if (role.Name.EndsWith("_group"))
                  return $"- **{role.Name.Replace("_group", "")}**\n";

               return "";
            });
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
