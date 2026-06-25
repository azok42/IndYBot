using Discord.Interactions;
using IndYLib.Services;
using IndYBot.Helpers;

namespace IndYBot.Modules;

public class IndyInfoModule : InteractionModuleBase<SocketInteractionContext>
{
   [SlashCommand("subjects", "Get a list of all valid subjects!")]
   public async Task SubjectsCommand()
   {
      var subjects = await IndyClient.GetActiveSubjectsAsync();

      await RespondAsync("Getting all subjects...", ephemeral: true);

      await MessageHelper.SendListMessageAsync(
            subjects,
            Context, 
            element => $"- {element.SubjectId} ({element.SubjectLong})\n",
            "# Subjects:\n");
   }

   [SlashCommand("specialindy", "Get a list of current special indys!")]
   public async Task SpecialIndyCommand()
   {
      var specialIndys = await IndyClient.GetSpecialIndyAsync();

      await RespondAsync("Getting all special indys...", ephemeral: true);

      await MessageHelper.SendListMessageAsync(
            specialIndys,
            Context, 
            element => $"- {element.TeacherId} {element.AreaOfExpertise} on {element.Day} ({element.StartDate} - {element.EndDate})\n",
            "# Special Indy:\n");
   }
}
