using Discord;
using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.Services;

namespace IndYBot.Modules;

[Group("role", "Manage roles")]
public class RoleModule : InteractionModuleBase<SocketInteractionContext> 
{
   private readonly LoginService _loginService;

   public RoleModule(LoginService loginService)
   {
      _loginService = loginService;
   }

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

   [SlashCommand("delete_group", "Deletes a group role!")]
   public async Task RemoveRoleCommand(
         [Summary("role", "The role to join!")] IRole role)
   {
      if (role.Name.EndsWith("_group"))
      {
         await role.DeleteAsync();
         await RespondAsync($"Successfully deleted role: {role.Name.Replace("_group", "")}");

         return;
      }

      await RespondAsync("Role is not a group!");
   }

   [RequireLogin]
   [SlashCommand("username", "Add you to a role called like your IndY-Username!")]
   public async Task AddRealNameRoleCommand(
         [Summary("color", "The color of you new role!")] string colorString = "")
   {
      var client = _loginService.GetClient(Context.Interaction.User.Id);
      var student = await client!.GetStudentAsync();

      if (!Color.TryParse(colorString, out var color))
      {
         await RespondAsync("Please use a real color! e.g.: #00FF00", ephemeral: true);
         return;
      }

      var role = await Context.Guild.CreateRoleAsync(student.First().Username, color: color);

      var user = Context.Interaction.User as IGuildUser;
      await user!.AddRoleAsync(role.Id);

      await RespondAsync($"Successfully added you to role {student.First().Username}!", ephemeral: true);
   }
}
