using Discord.Interactions;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.Services;
using IndYLib.Models.Entry;
using IndYLib.Interfaces;
using IndYLib.Exceptions;

namespace IndYBot.Modules;

[Group("absence_rank", "Various commands related to the absence ranks!")]
public class AbsenceRankModule : InteractionModuleBase<SocketInteractionContext>
{
   private IIndyClient? _client;
   private readonly LoginService _loginService;

   public AbsenceRankModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      _client = _loginService.GetClient(Context.Interaction.User.Id);
   }

   [RequireLogin]
   [SlashCommand("get", "Get your or someone else's absence rank and hours")]
   public async Task AbsenceRankCommand(
         [Summary("", "")] string? name = null)
   {
      AbsenceRank rank;
      if (string.IsNullOrEmpty(name))
      {
         rank = await _client!.GetAbsenceRankAsync();

         await RespondAsync($"You have {rank.AbsenceCount} absence hours and are rank #{rank.Rank}", ephemeral: true);
      }
      else
      {
         try
         {
            rank = await _client!.GetAbsenceRankAsync(name);
         }
         catch (ArgumentException)
         {
            await RespondAsync($"Name parameter must consist of firstname and lastname!", ephemeral: true);
            return;
         }
         catch (NotFoundException)
         {
            await RespondAsync($"Student {name} was not found!", ephemeral: true);
            return;
         }

         await RespondAsync($"Student **{name}** has {rank.AbsenceCount} absence hours and is rank #{rank.Rank}", ephemeral: true);
      }
   }
}
