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

   public enum Day
   {
      [ChoiceDisplay("Monday")]
      Mo,

      [ChoiceDisplay("Wendsday")]
      Mi,

      [ChoiceDisplay("Friday")]
      Fr
   }

   public enum Hour
   {
      [ChoiceDisplay("3")]
      Hour3 = 3,

      [ChoiceDisplay("4")]
      Hour4 = 4
   }

   [SlashCommand("hours", "get a list of all indy hours")]
   public async Task IndyHourCommand(
         [Summary("hour", "Show only hours in hour")] Hour? hour = null,
         [Summary("day", "Show only hours on day")] Day? day = null,
         [Summary("teacher", "Only show hours from teacher")]
         [Autocomplete(typeof(TeacherAutoCompleteHandler))] string teacherId = "")
   {
      await RespondAsync("Getting all indy hours...", ephemeral: true);

      var indyHours = await IndyClient.GetIndyHoursAsync();

      if (hour != null)
         indyHours = indyHours.Where(x => (Hour) x.Hour == hour.Value).ToList();

      if (day != null)
      {
         var dayName = day switch
         {
            Day.Mo => "Mo",
            Day.Mi => "Mi",
            Day.Fr => "Fr",
            _ => ""
         };

         indyHours = indyHours.Where(x => dayName.Equals(x.DayName)).ToList();
      }

      if (!string.IsNullOrWhiteSpace(teacherId))
         indyHours = indyHours.Where(x => x.TeacherId.Equals(teacherId)).ToList();

      await MessageHelper.SendListMessageAsync(
            indyHours,
            Context,
            e => $"- {e.TeacherId} ({e.TeacherName}) in {e.Room} on {e.DayName} {e.Hour}\n",
            "# Indy hours:\n");
   }
}
