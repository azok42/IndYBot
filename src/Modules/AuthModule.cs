using Discord.Interactions;
using IndYBot.Modules.Services;

namespace IndYBot.Modules;

[Group("get", "Getters")]
public class AuthModule : InteractionModuleBase<SocketInteractionContext>
{
   public readonly LoginService _loginService; 

   public AuthModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   [SlashCommand("student", "Get info about yourself!")]
   public async Task StudentInfoCommand()
   {
      var client = (await _loginService.getClient(Context.Interaction.User.Id).GetStudentAsync()).First();

      await RespondAsync($"{client.StudentId}: {client.Firstname} {client.Lastname} {client.Class} ({client.EMail})", ephemeral: true);
   }
}
