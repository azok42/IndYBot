using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using IndYLib.Interfaces;

namespace IndYBot.Modules;

[Group("info", "Getters with needed login")]
public class AuthModule : InteractionModuleBase<SocketInteractionContext>
{
   public readonly LoginService _loginService; 

   private IIndyClient? _client = null;

   public AuthModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      try
      {
         _client = _loginService.GetClient(Context.Interaction.User.Id);
      }
      catch (KeyNotFoundException)
      {
         RespondAsync("## [ERROR] Login needed", ephemeral: true);
         throw;
      }
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
      await RespondAsync("Getting teachers...");

      var teachers = (await _client!.GetTeachersAsync());

      await MessageHelper.SendListMessageAsync(
            teachers,
            Context,
            e => $"- **{e.TeacherId}** ({e.Firstname} {e.Lastname}): {e.Expertises}\n",
            "# Teachers:\n");
   }

   [SlashCommand("teacherabsences", "Get all teacher absences!")]
   public async Task TeacherAbsencesCommand()
   {
      await RespondAsync("Getting teacher-absences...");

      var teacherAbsences = await _client!.GetTeacherAbsencesAsync();

      await MessageHelper.SendListMessageAsync(
            teacherAbsences,
            Context,
            e => $"- **{e.TeacherId}:** {e.Hour} {e.Date}\n",
            "# Teacher Absences:\n");
   }
   
   [SlashCommand("statuses", "Get the status of each indy day in range!")]
   public async Task DayStatusesCommand(
         [Summary("month", "The month to get statuses for")] int month = -1)
   {
      await RespondAsync("Getting statuses...");

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

      await MessageHelper.SendListMessageAsync(
            statuses,
            Context,
            e => $"-  **{e.Date} {e.DayName}:** {e.Status.ToString()}\n",
            "# Statuses:\n");
   }
}
