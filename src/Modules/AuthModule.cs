using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
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
         _client = _loginService.GetClient(Context.Interaction.User.Id);
      }
      catch (KeyNotFoundException)
      {
         RespondAsync("## [ERROR] Login needed", ephemeral: true);
         throw;
      }
   }

   [SlashCommand("student", "Get info about yourself!")]
   public async Task StudentInfoCommand()
   {
      var student = (await _client!.GetStudentAsync()).First();

      await RespondAsync($"{student.StudentId}: {student.Firstname} {student.Lastname} {student.Class} ({student.EMail})", ephemeral: true);
   }

   [SlashCommand("teachers", "Get a list of all teachers!")]
   public async Task TeachersCommand()
   {
      await RespondAsync("Getting teachers...");

      var teachers = (await _client!.GetTeachersAsync());

      await MessageHelper.SendListMessageAsync(
            teachers,
            Context,
            e => $"- **{e.TeacherId}** ({e.Firstname} {e.Lastname}): {e.Expertises}\n",
            "# Teachers:\n");
   }
}
