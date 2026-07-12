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

   [SlashCommand("entries", "Get made entries for a specific date!")]
   public async Task DayEntriesCommand(
            [Summary("date", "Date to get entries for!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date)
   {
      await DeferAsync();

      FullRetured fullRetured;
      List<DayStatus> statusList;
      try
      {
         fullRetured = await _client!.GetEntriesAsync(DateOnly.Parse(date));
         statusList = await _client!.GetDayStatusesAsync(DateOnly.Parse(date), DateOnly.Parse(date).AddDays(1));
      }
      catch (InvalidIndyDayException)
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"**[ERROR]** {date} is not a valid IndY-Day!");
         throw;
      }

      var status = statusList.First().Status;
      Color color = GetColorForStatus(status);

      var (hour3Content, hour3Disabled) = ProcessHourData(fullRetured.Hour3, status);
      var (hour4Content, hour4Disabled) = ProcessHourData(fullRetured.Hour4, status);

      var embed = new EmbedBuilder()
         .WithTitle($"Entries for date {date}")
         .WithAuthor(new EmbedAuthorBuilder().WithName("IndYBot"))
         .WithColor(color)
         .AddField("Hour 3", hour3Content, true)
         .AddField("Hour 4", hour4Content, true)
         .Build();

      var buttons = BuildEntryButtons(hour3Disabled, hour4Disabled);
      await ModifyOriginalResponseAsync(x =>
            {
               x.Embed = embed;
               x.Components = buttons;
            });
   }

   private Color GetColorForStatus(Status status)
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

   private MessageComponent BuildEntryButtons(bool hour3Disabled, bool hour4Disabled)
   {
      return new ComponentBuilder()
         .WithButton(
               label: "Make entry for hour 3",
               customId: "quickEntryHour3",
               style: ButtonStyle.Primary,
               disabled: hour3Disabled)
         .WithButton(
               label: "Make entry for hour 4",
               customId: "quickEntryHour4",
               style: ButtonStyle.Primary,
               disabled: hour4Disabled)
         .Build();
   }

   private (string FieldContent, bool buttonDisabled) ProcessHourData(List<Returned> hour, Status status)
   {
      if (hour.Any())
         return (MakeEntryHourFieldContent(hour.First()), true);

      if (status == Status.EntriesMissing)
         return ("No entry made!", true);

      return ("No entry made yet!", false);
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
