using Discord.Interactions;
using IndYLib.Services;
using IndYBot.Helpers;
using IndYBot.Modules.AutocompleteHandlers;
using System.Globalization;

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

   [SlashCommand("specialindy", "Get a list of current Special-IndYs!")]
   public async Task SpecialIndyCommand(
         [Summary("teacher-id", "Only show Special-IndYs from teacher")]
         [Autocomplete(typeof(TeacherAutocompleteHandler))] string teacherId = "")
   {
      await RespondAsync("Getting all Special-IndYs...", ephemeral: true);

      var specialIndys = await IndyClient.GetSpecialIndyAsync();

      string firstMsg = "# Special-IndYs:\n";
      if (!string.IsNullOrWhiteSpace(teacherId))
      {
         specialIndys = specialIndys.Where(x => x.TeacherId.Equals(teacherId, StringComparison.OrdinalIgnoreCase)).ToList();
         firstMsg = $"# Special-IndY for {teacherId}:\n";
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

   [SlashCommand("hours", "Get a list of all IndY-Hours!")]
   public async Task IndyHourCommand(
         [Summary("hour", "Show only hours in hour")] Hour? hour = null,
         [Summary("day", "Show only hours on day")] Day? day = null,
         [Summary("teacher", "Only show hours from teacher")]
         [Autocomplete(typeof(TeacherAutocompleteHandler))] string teacherId = "")
   {
      await RespondAsync("Getting all IndY-Hours...", ephemeral: true);

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
            "# IndY-Hours:\n");
   }

   [Group("studentcount", "Get studentcount for a specific day!")]
   public class StudentCountSubCommandModule : InteractionModuleBase<SocketInteractionContext>
   {
      [SlashCommand("list", "Get studentcounts in form of a list!")] 
      public async Task StudentCountListCommand(
            [Summary("date", "Date to get studentcount for!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
            [Summary("hour", "Show only hours in hour")] Hour? hour = null)
      {
         await RespondAsync("Getting studentcount...", ephemeral: true);

         var studentcounts = await IndyClient.GetStudentCountAsync(DateOnly.Parse(date));

         if (hour != null)
            studentcounts = studentcounts.Where(x => (Hour) x.Hour == hour.Value).ToList();

         await MessageHelper.SendListMessageAsync(
               studentcounts,
               Context,
               e => $"- {e.TeacherId} {e.Hour}: {e.Count}\n",
               "# Studentcounts:\n");
      }

      [SlashCommand("plot", "Get studentcounts in form of a plot!")]
      public async Task StudentCountPlotCommand(
            [Summary("date", "Date to get studentcount for!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
            [Summary("hour", "Show only hours in hour")] Hour? hour = null,
            [Summary("darkMode", "Wether to enable dark mode or not. Default: true")] bool isDarkMode = true)
      {
         await RespondAsync("Getting studentcount...", ephemeral: true);

         var studentcounts = await IndyClient.GetStudentCountAsync(DateOnly.Parse(date));
         var indyHours = await IndyClient.GetIndyHoursAsync();

         studentcounts.Sort((a, b) => a.TeacherId.CompareTo(b.TeacherId));
         indyHours.Sort((a, b) => a.TeacherId.CompareTo(b.TeacherId));

         if (hour != null)
         {
            studentcounts = studentcounts
               .Where(x => (Hour) x.Hour == hour.Value)
               .ToList();

            indyHours = indyHours
               .Where(x => (Hour) x.Hour == hour.Value)
               .ToList();
         }

         var ids = studentcounts.Select(x => x.TeacherId).ToList();
         var counts = studentcounts.Select(x => (double) x.Count).ToList();
         var maxCount = indyHours
            .Where(x => x.DayName.Equals(DateOnly.Parse(date).ToString("ddd", new CultureInfo("de-DE"))))
            .Select(sc => {
                  var matchingHour = indyHours.FirstOrDefault(ih => ih.TeacherId.Equals(sc.TeacherId));
                  return matchingHour != null ? (double) matchingHour.StudentLimit : 0.0;
            })
            .ToList();

         await PlotHelper.SendBasicPlot(
               Context,
               ids, counts,
               "Studentcounts",
               "Teachers",
               "Counts", 
               secondYValues: maxCount,
               isDark: isDarkMode);
      }
   }

   [SlashCommand("indydays", "Get all days from 1 month range which are IndY-Days!")]
   public async Task IndyDaysCommand(
         [Summary("month", "The month to get IndY-Days for")] int month = -1)
   {
      await RespondAsync("Getting IndY-Days...", ephemeral: true);

      var today = DateOnly.FromDateTime(DateTime.Today);

      DateOnly startDate;
      DateOnly endDate;

      if (month == -1)
      {
         startDate = today.AddDays(-15);
         endDate = today.AddDays(15);
      }
      else
      {
         startDate = new (today.Year, month, 1);
         endDate = new (today.Year, month, DateTime.DaysInMonth(today.Year, month));
      }

      var indyDays = await IndyClient.GetIndyDaysAsync(startDate, endDate);

      await MessageHelper.SendListMessageAsync(
            indyDays,
            Context, 
            e => $"- {e.DayName} {e.Date}\n",
            "# IndY-Days:\n");
   }

}
