using Discord.Interactions;
using IndYLib.Services;
using IndYBot.Helpers;
using IndYBot.Modules.AutocompleteHandlers;

namespace IndYBot.Modules;

[Group("get", "Getters")]
public class IndyInfoModule : InteractionModuleBase<SocketInteractionContext>
{
   [SlashCommand("subjects", "Get a list of all valid subjects!")]
   public async Task SubjectsCommand()
   {
      await RespondAsync("Getting all subjects...", ephemeral: true);

      var subjects = await IndyClient.GetActiveSubjectsAsync();

      await MessageHelper.SendListMessageAsync(
            subjects,
            Context, 
            element => $"- {element.SubjectId} ({element.SubjectLong})\n",
            "# Subjects:\n");
   }

   [SlashCommand("specialindy", "Get a list of current special indys!")]
   public async Task SpecialIndyCommand(
         [Summary("teacher-id", "Only show special indys from teacher")]
         [Autocomplete(typeof(TeacherAutoCompleteHandler))] string teacherId = "")
   {
      await RespondAsync("Getting all special indys...", ephemeral: true);

      var specialIndys = await IndyClient.GetSpecialIndyAsync();

      string firstMsg = "# Special Indys:\n";
      if (!string.IsNullOrWhiteSpace(teacherId))
      {
         specialIndys = specialIndys.Where(x => x.TeacherId.Contains(teacherId, StringComparison.OrdinalIgnoreCase)).ToList();
         firstMsg = $"# Special Indy for {teacherId}:\n";
      }

      await MessageHelper.SendListMessageAsync(
            specialIndys,
            Context, 
            element => $"- {element.TeacherId} \t {element.AreaOfExpertise} on {element.Day} {element.Hour} ({element.StartDate} - {element.EndDate})\n",
            firstMsg);
   }
}
