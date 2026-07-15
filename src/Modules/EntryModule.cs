using Discord;
using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYLib.Interfaces;
using IndYLib.Exceptions;
using IndYLib.Models.Entry;

namespace IndYBot.Modules;

[Group("entry", "Make a new manual entry just for you!")]
public class EntryModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;

   private IIndyClient? _client = null;

   public EntryModule(LoginService loginService)
   {
      _loginService = loginService;
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
