using Discord;
using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYBot.Helpers;
using IndYLib.Interfaces;
using IndYLib.Models;
using IndYLib.Models.Entry;
using IndYLib.Exceptions;

namespace IndYBot.Modules;

[RequireLogin]
[Group("info", "Getters with needed login")]
public class AuthGetterModule : InteractionModuleBase<SocketInteractionContext>
{
   public readonly LoginService _loginService; 

   private IIndyClient? _client = null;

   public AuthGetterModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      _client = _loginService.GetClient(Context.Interaction.User.Id);
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
      await RespondAsync("# Teachers:");

      var teachers = (await _client!.GetTeachersAsync());

      if (teachers == null || teachers.Count == 0)
         await ModifyOriginalResponseAsync(x => x.Content = "No teachers found!");
      else
         await MessageHelper.SendListMessageAsync(
               teachers,
               Context,
               e => $"- **{e.TeacherId}** ({e.Firstname} {e.Lastname}): {e.Expertises}\n");
   }

   [SlashCommand("teacherabsences", "Get all teacher absences!")]
   public async Task TeacherAbsencesCommand()
   {
      await RespondAsync("# Teacher-absences:");

      var teacherAbsences = await _client!.GetTeacherAbsencesAsync();

      if (teacherAbsences == null || teacherAbsences.Count == 0)
         await ModifyOriginalResponseAsync(x => x.Content = "No absences found!");
      else
         await MessageHelper.SendListMessageAsync(
               teacherAbsences,
               Context,
               e => $"- **{e.TeacherId}:** {e.Hour} {e.Date}\n");
   }
   
   [SlashCommand("statuses", "Get the status of each indy day in range!")]
   public async Task DayStatusesCommand(
         [Summary("month", "The month to get statuses for")] int month = -1)
   {
      await RespondAsync("# Statuses:");

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

      var statuses = await _client!.GetDayStatusesAsync(startDate, endDate);

      if (statuses == null || statuses.Count == 0)
         await ModifyOriginalResponseAsync(x => x.Content = "No statuses found!");
      else
         await MessageHelper.SendListMessageAsync(
               statuses,
               Context,
               e => $"-  **{e.Date} {e.DayName}:** {e.Status.ToString()}\n");
   }

   [SlashCommand("schedule", "Get an overview of IndY-Day's this week!", ignoreGroupNames: true)]
   public async Task ScheduleCommand(
         [Summary("date", "A date in the week to want to get infos for!")]
         [Autocomplete(typeof(IndyDayAutocompleteHandler))] string dateString = "")
   {
      await DeferAsync();

      DateOnly today;

      if (string.IsNullOrEmpty(dateString))
         today = DateOnly.FromDateTime(DateTime.Today);
      else
         today = DateOnly.Parse(dateString);

      var mondayOffset = (int)today.DayOfWeek == 0 ? -6 : 1 - (int)today.DayOfWeek;
      var monday = today.AddDays(mondayOffset);

      List<Embed> embeds = new();

      int[] targetDays = { 0, 2, 4 };
      foreach (var offset in targetDays)
      {
         var date = monday.AddDays(offset);

         try
         {
            var retured = await _client!.GetEntriesAsync(date);
            var statusList = await _client!.GetDayStatusesAsync(date, date.AddDays(1));
            var status = statusList.First().Status;

            var embed = await GetEmbedForDay(date.ToString(), status, retured);
            embeds.Add(embed);
         }
         catch (InvalidIndyDayException)
         {
            embeds.Add(GenerateNoEntriesEmbed(date.ToString()));
            continue;
         }
      }

      await ModifyOriginalResponseAsync(
            x => x.Embeds = embeds.ToArray()
         );
   }

   private Embed GenerateNoEntriesEmbed(string dateString)
   {
      return new EmbedBuilder()
         .WithTitle($"Entries for date {dateString}")
         .WithAuthor(new EmbedAuthorBuilder().WithName("IndYBot"))
         .WithColor(Color.Teal)
         .AddField("This day is not a IndY-Day!", "This might be due to holidays or other this which replace IndY", true)
         .Build();

   }

   private async Task<Embed> GetEmbedForDay(string date, Status status, FullRetured returned)
   {
      var color = GetColorFromStatus(status);

      var hour3Content = GetHourContent(returned.Hour3, status);
      var hour4Content = GetHourContent(returned.Hour4, status);
      
      return new EmbedBuilder()
         .WithTitle($"Entries for date {date}")
         .WithAuthor(new EmbedAuthorBuilder().WithName("IndYBot"))
         .WithColor(color)
         .AddField("Hour 3", hour3Content, true)
         .AddField("Hour 4", hour4Content, true)
         .Build();
   }

   private string GetHourContent(List<Returned> hour, Status status)
   {
      if (status == Status.EntriesMissing)
         return "No entry made!";

      if (hour.Any())
      if (status == Status.FullySigned || status == Status.Cancelled || status == Status.NotSigned)
         return MakeEntryHourFieldContent(hour.First());
      else
         return MakeEntryHourFieldContent(hour.First());

      return "No entry made yet!";

   }

   private Color GetColorFromStatus(Status status)
   {
      return status switch
      {
         Status.FullySigned => Color.Green,
         Status.Open => Color.LightGrey,
         Status.NotSigned => Color.LightOrange,
         Status.EntriesMissing => Color.Red,
         Status.AbsenceEntries => Color.DarkOrange,
         Status.Cancelled => Color.DarkPurple,
         Status.Unkown => Color.Teal,
         _ => throw new Exception($"Invalid status recieved: {status}")
      };
   }


   private string MakeEntryHourFieldContent(Returned entry)
   {
      return entry switch
      {
         NormalReturned normal =>
            $"- **Type**: Normal\n- **Subject:** {normal.Subject}\n- **Teacher/Room**: {normal.TeacherId} {normal.Room}\n- **Signed**: {normal.IsSigned}\n- **Activity**: {normal.Activity}\n",

         AbsenceReturned absence =>
            $"- **Type**: Absence\n- **Signed**: {absence.IsSigned}\n",

         SchoolEventReturned schoolevent =>
            $"- **Type**: Schoolevent\n- **Teacher**: {schoolevent.TeacherId}\n- **Description**: {schoolevent.Description}\n- **Signed**: {schoolevent.IsSigned}\n",

         SpecialReturned special =>
            $"- **Type**: Special-IndY\n- **Teacher/Room**: {special.TeacherId} {special.Room}\n- **Activity**: {special.Activity}\n- **Range**: {special.StartDate} - {special.EndDate}\n- **Subject**: {special.Subject}\n- **Signed**: {special.IsSigned}\n",

         _ => throw new Exception($"Invalid returned type!")
      };
   }
}
