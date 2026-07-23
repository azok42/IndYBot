using Discord;
using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYLib.Interfaces;
using IndYLib.Exceptions;
using IndYLib.Models.Entry;
using Dapper;

namespace IndYBot.Modules;

[Group("entry", "Make a new manual entry just for you!")]
public class EntryModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;
   private readonly SQLHelper _sqlHelper;

   private IIndyClient? _client = null;

   public EntryModule(LoginService loginService, SQLHelper sqlHelper)
   {
      _loginService = loginService;
      _sqlHelper = sqlHelper;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      _client = _loginService.GetClient(Context.Interaction.User.Id);
   }

   [RequireLogin]
   [SlashCommand("normal", "Make a normal entry!")]
   public async Task NormalEntryCommand(
            [Summary("date", "Date for the entry!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
            [Summary("teacher", "Teacher where to make the entry!")]
            [Autocomplete(typeof(TeacherAutocompleteHandler))] string teacherId,
            [Summary("subject", "The subject of the entry!")]
            [Autocomplete(typeof(SubjectAutocompleteHandler))] string subject,
            [Summary("activity", "Your activity of the entry!")] string activity,
            [Summary("hour", "The hour of the entry. Leave empty for both hours!")] GetterModule.Hour? hour = null)
   {
      await DeferAsync(ephemeral: true);

      List<Normal> entries = new();

      bool success = await TryMakeEntry(async () =>
      {
         if (hour == null)
            entries = await _client!.MakeNormalEntryAsync(
                  DateOnly.Parse(date),
                  teacherId,
                  subject,
                  activity);
         else
            entries.Add(await _client!.MakeNormalEntryAsync(
                     DateOnly.Parse(date),
                     (int) hour.Value,
                     teacherId,
                     subject,
                     activity));
      });

      if (!success) return;

      await ModifyOriginalResponseAsync(x => {
               x.Content = $"Successfully made normal entries for {entries.First().Date}";
               x.Flags = MessageFlags.Ephemeral;
            });
   }

   [RequireLogin]
   [SlashCommand("absence", "Make a absence entry!")]
   public async Task AbsenceEntryCommand(
            [Summary("date", "Date for the entry!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
            [Summary("hour", "The hour of the entry. Leave empty for both hours!")] GetterModule.Hour? hour = null)
   {
      await DeferAsync(ephemeral: true);

      List<Absence> entries = new();

      bool success = await TryMakeEntry(async () =>
      {
         if (hour == null)
            entries = await _client!.MakeAbsenceEntryAsync(DateOnly.Parse(date));
         else
            entries.Add(await _client!.MakeAbsenceEntryAsync(DateOnly.Parse(date), (int) hour.Value));
      });

      if (!success) return;

      await ModifyOriginalResponseAsync(x => {
               x.Content = $"Successfully made absence entries for {entries.First().Date}";
               x.Flags = MessageFlags.Ephemeral;
            });
   }

   [RequireLogin]
   [SlashCommand("event", "Make a schoolevent entry!")]
   public async Task SchooleventEntryCommand(
            [Summary("date", "Date for the entry!")]
            [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
            [Summary("teacher", "Teacher where to make the entry!")]
            [Autocomplete(typeof(TeacherAutocompleteHandler))] string teacherId,
            [Summary("description", "Your description of the event!")] string description,
            [Summary("hour", "The hour of the entry. Leave empty for both hours!")] GetterModule.Hour? hour = null)
   {
      await DeferAsync(ephemeral: true);

      List<SchoolEvent> entries = new();

      bool success = await TryMakeEntry(async () =>
      {
         if (hour == null)
            entries = await _client!.MakeSchoolEventEntryAsync(
                  DateOnly.Parse(date),
                  teacherId,
                  description);
         else
            entries.Add(await _client!.MakeSchoolEventEntryAsync(
                     DateOnly.Parse(date),
                     (int) hour.Value,
                     teacherId,
                     description));
      });

      if (!success) return;

      await ModifyOriginalResponseAsync(x => {
               x.Content = $"Successfully made schoolevent entries for {entries.First().Date}";
               x.Flags = MessageFlags.Ephemeral;
            });
   }

   [RequireLogin]
   [SlashCommand("standard", "Make a normal entry with options from your standards!")]
   public async Task StandardEntryCommand(
         [Summary("date", "The date of your entry!")]
         [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
         [Summary("teacher", "Teacher where to make the entry! Used to override standard!")]
         [Autocomplete(typeof(TeacherAutocompleteHandler))] string? teacherId = null,
         [Summary("subject", "The subject of the entry! Used to override standard!")]
         [Autocomplete(typeof(SubjectAutocompleteHandler))] string? subject = null,
         [Summary("description", "Your description of the entry! Used to override standard!")] string? description = null)
   {
      await DeferAsync(ephemeral: true);

      var parsedDate = DateOnly.Parse(date);   
      var userId = Context.Interaction.User.Id;

      var con = _sqlHelper.CreateConnection();
      var sql = "SELECT type, value FROM user_standard WHERE id = @Id;";
      var queryResult = await con.QueryAsync<(string Type, string Value)>(sql, new { Id = userId });
      var standards = queryResult.ToDictionary(x => x.Type, x => x.Value);

      string dayName = parsedDate.DayOfWeek.ToString();
      string? GetStandardValue(string standard)
      {
         if (standards.TryGetValue($"{dayName}{standard}", out var dayValue)) return dayValue;
         if (standards.TryGetValue($"Global{standard}", out var globalValue)) return globalValue;
         return null;
      }

      var finalTeacher = !string.IsNullOrEmpty(teacherId) ? teacherId : GetStandardValue("Teacher");
      var finalSubject = !string.IsNullOrEmpty(subject) ? subject : GetStandardValue("Subject");
      var finalDescription = !string.IsNullOrEmpty(description) ? description : GetStandardValue("Description");

      if (
            string.IsNullOrEmpty(finalTeacher) ||
            string.IsNullOrEmpty(finalSubject) || 
            string.IsNullOrEmpty(finalDescription))
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"Could not find all standards! Please provide either missing parameters now or add standards for them using the '/standard set' command!");
         return;
      }

      List<Normal> entries = new();

      bool success = await TryMakeEntry(async () =>
      {
         entries = await _client!.MakeNormalEntryAsync(
               parsedDate,
               finalTeacher,
               finalSubject,
               finalDescription);
      });

      if (!success)
         return;

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully made entry for date {date}");
   }

   private async Task<bool> TryMakeEntry(Func<Task> action)
   {
      try {
         await action();
         return true;
      }
      catch (NotFoundException)
      {
         await ModifyOriginalResponseAsync(x => x.Content = "No hour for this teacher on this day!");

         return false;
      }
      catch (InvalidIndyDayException)
      {
         await ModifyOriginalResponseAsync(x => x.Content = "Not a valid IndY-Day!");

         return false;
      }
      catch (Exception)
      {
         await ModifyOriginalResponseAsync(x => x.Content = "Something went wrong!");

         return false;
      }
   }
}
