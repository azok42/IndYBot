using Discord;
using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;

namespace IndYBot.Modules;

public class NicknameModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;

   public NicknameModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   [RequireLogin]
   [SlashCommand("setnickname", "Set your server nickname to you name on IndY!")]
   public async Task SetNicknameCommand()
   {
      var client = _loginService.GetClient(Context.Interaction.User.Id);
      var student = await client!.GetStudentAsync();

      var user = Context.Interaction.User as IGuildUser;
      await user!.ModifyAsync(x => x.Nickname = student.First().Firstname + " " + student.First().Lastname);

      await RespondAsync("Successfully changed nickname!");
   }
}
