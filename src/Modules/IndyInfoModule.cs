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
      var subjects = await IndyClient.GetActiveSubjectsAsync();

      await RespondAsync("Getting all subjects...", ephemeral: true);

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
      var specialIndys = await IndyClient.GetSpecialIndyAsync();

      if (!string.IsNullOrWhiteSpace(teacherId))
         specialIndys = specialIndys.Where(x => x.TeacherId.Contains(teacherId, StringComparison.OrdinalIgnoreCase)).ToList();

      Console.WriteLine(specialIndys.Count);

      await RespondAsync("Getting all special indys...", ephemeral: true);

      await MessageHelper.SendListMessageAsync(
            specialIndys,
            Context, 
            element => $"- {element.TeacherId} \t {element.AreaOfExpertise} on {element.Day} {element.Hour} ({element.StartDate} - {element.EndDate})\n",
            "# Special Indy:\n");
   }
}
