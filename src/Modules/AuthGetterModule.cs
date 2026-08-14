using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;
using IndYBot.Helpers;
using IndYLib.Interfaces;

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
}
