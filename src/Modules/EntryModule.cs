using Discord;
using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using IndYBot.Modules.Modals;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYLib.Interfaces;
using IndYLib.Exceptions;
using IndYLib.Models;
using IndYLib.Models.Entry;
using Dapper;

namespace IndYBot.Modules;

[Group("entry", "Commands to create or view a single entry!")]
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

   [RequireLogin]
   [SlashCommand("view", "Get made entries for a specific date!")]
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

      var buttons = BuildEntryButtons(hour3Disabled, hour4Disabled, date);
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

   private MessageComponent BuildEntryButtons(bool hour3Disabled, bool hour4Disabled, string date)
   {
      return new ComponentBuilderV2()
         .WithActionRow(
            new List<IMessageComponentBuilder> {
               new ButtonBuilder(
                  label: "Make entry for hour 3",
                  customId: $"entry:3:{date}",
                  style: ButtonStyle.Primary,
                  isDisabled: hour3Disabled
               ),
               new ButtonBuilder(
                  label: "Make entry for hour 4",
                  customId: $"entry:4:{date}",
                  style: ButtonStyle.Primary,
                  isDisabled: hour4Disabled
               )
            }
         )
         .WithActionRow(
            new List<IMessageComponentBuilder> {
               new ButtonBuilder(
                  label: "Make entries for both hours",
                  customId: $"entry:34:{date}",
                  style: ButtonStyle.Primary,
                  isDisabled: hour3Disabled || hour4Disabled
               )
            }
         )
         .Build();
   }

   [ComponentInteraction("entry:*:*", ignoreGroupNames: true)]
   public async Task HandleEntryButtons(string hour, string date)
   {
      await RespondWithModalAsync<EntryModal>(
         "entry", 
         new EntryModal(
            date: date, 
            hour: Enum.Parse<Hour>("Hour"+hour)
         )
      );
   }

   [ModalInteraction("entry", ignoreGroupNames: true)]
   public async Task HandleEntryModal(EntryModal modal)
   {
      await DeferAsync(ephemeral: true);

      if (!await MakeEntry(modal))
      {
         await ModifyOriginalResponseAsync(x => x.Content = "Failed to make entry!");
         return;
      }

      await ModifyOriginalResponseAsync(x => x.Content = "Successfully made entry!");
   }

   private async Task<bool> MakeEntry(EntryModal entry)
   {
      if (!DateOnly.TryParse(entry.Date, out var date))
      {
         await FollowupAsync("Date parameter is not a valid IndY-Day!", ephemeral: true);
         return false;
      }

      return await TryMakeEntry(async () =>
      {
         if (entry.Hour == null)
            await _client!.MakeNormalEntryAsync(date, entry.TeacherId!, entry.Subject!, entry.Activity!);
         else
            await _client!.MakeNormalEntryAsync(date, (int) entry.Hour.First(), entry.TeacherId!, entry.Subject!, entry.Activity!);
      });
   }

   private (string FieldContent, bool ButtonDisabled) ProcessHourData(List<Returned> hour, Status status)
   {
      if (status == Status.EntriesMissing)
         return ("No entry made!", true);

      if (hour.Any())
      if (status == Status.FullySigned || status == Status.Cancelled || status == Status.NotSigned)
         return (MakeEntryHourFieldContent(hour.First()), true);
      else
         return (MakeEntryHourFieldContent(hour.First()), false);

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
