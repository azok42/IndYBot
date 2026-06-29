using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYLib.Interfaces;

namespace IndYBot.Modules;

[Group("info", "Getters with needed login")]
public class AuthModule : InteractionModuleBase<SocketInteractionContext>
{
   public readonly LoginService _loginService; 

   private IIndyClient? _client = null;

   public AuthModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      try
      {
         _client = _loginService.getClient(Context.Interaction.User.Id);
      }
      catch (KeyNotFoundException)
      {
         RespondAsync("## Login needed", ephemeral: true);
         throw;
      }
   }

   [SlashCommand("student", "Get info about yourself!")]
   public async Task StudentInfoCommand()
   {
      var student = (await _client!.GetStudentAsync()).First();

      await RespondAsync($"{student.StudentId}: {student.Firstname} {student.Lastname} {student.Class} ({student.EMail})", ephemeral: true);
   }
}
