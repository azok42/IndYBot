using Discord;
using Discord.Interactions;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYBot.Modules.Services;
using IndYBot.Modules.Modals;
using IndYLib.Interfaces;
using IndYLib.Exceptions;

namespace IndYBot.Modules;

public class GroupEntryModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;
   private readonly QuickEntryService _quickEntryService;

   private IIndyClient? _client = null;

   public GroupEntryModule(LoginService loginService, QuickEntryService quickEntryService)
   {
      _loginService = loginService;
      _quickEntryService = quickEntryService;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      _client = _loginService.GetClient(Context.Interaction.User.Id);
   }

   public enum EntryType
   {
      Normal,
      Absence,
      Schoolevent
   }

   [RequireLogin]
   [SlashCommand("groupentry", "Make a new entry for everyone!")]
   public async Task GroupEntryCommand(
         [Summary("type", "The type of your group entry!")] EntryType type,
         [Summary("date", "The date of your group entry!")]
         [Autocomplete(typeof(IndyDayAutocompleteHandler))] string date,
         [Summary("teacher", "Teacher where to make the group entry! Ignored for absence entries.")]
         [Autocomplete(typeof(TeacherAutocompleteHandler))] string teacherId = "",
         [Summary("subject", "The subject of the group entry! Ignored for absence/event entries.")]
         [Autocomplete(typeof(SubjectAutocompleteHandler))] string subject = "",
         [Summary(
            "description",
            "A description of the group entry! Leave empty to let users choose. Ignored for absence entries.")]
         string description = "",
         [Summary("hour", "The hour of your group entry. Leave empty for both hours!")] GetterModule.Hour? hour = null,
         [Summary("reason", "Why was this entry made! Leave empty for none.")] string reason = "")
   {
      await DeferAsync();

      Color color = Color.DarkGrey;
      try
      {
         color = GetColorFromType(type);
      }
      catch (ArgumentOutOfRangeException)
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"Invalid type {type.ToString()}");
         return;
      }

      Guid entryToken = Guid.NewGuid();

      var entryData = new QuickEntry
      {
         Type = type,
         Date = date,
         TeacherId = teacherId,
         Subject = subject,
         Description = description,
         Hour = hour
      };

      _quickEntryService.PendingEntries.TryAdd(entryToken, entryData);

      ComponentBuilder component = new ComponentBuilder();

      if (string.IsNullOrEmpty(description))
         component = component.WithButton("Make entry", $"add_description:{entryToken}", ButtonStyle.Success);
      else
         component = component
            .WithButton("Make entry", $"quickentry:{entryToken}", ButtonStyle.Success)
            .WithButton("Make entry with new description", $"add_description:{entryToken}", ButtonStyle.Primary);

      var fields = new List<EmbedFieldBuilder>();

      if (!string.IsNullOrEmpty(reason))
      {
         var reasonFieldBuilder = new EmbedFieldBuilder().WithName("Reason").WithValue(reason);
         fields.Add(reasonFieldBuilder);
      }

      var optionsFieldBuilder = MakeFieldBuilderFromEntry(entryData);
      fields.Add(optionsFieldBuilder);

      var embed = new EmbedBuilder()
         .WithTitle($"{type.ToString()} group entry for {date}!")
         .WithAuthor(new EmbedAuthorBuilder().WithName(Context.Interaction.User.Username))
         .WithColor(color)
         .WithDescription("You can quickly make an entry with the following\noptions using the buttons below!")
         .WithFields(fields);

      await ModifyOriginalResponseAsync(x => 
            {
               x.Embed = embed.Build();
               x.Components = component.Build();
            });
   }

   private Color GetColorFromType(EntryType type)
   {
      return type switch
      {
         EntryType.Normal => Color.Green,
         EntryType.Schoolevent => Color.LightOrange,
         EntryType.Absence => Color.DarkRed,
         _ => throw new ArgumentOutOfRangeException($"Invalid entry type {type.ToString()}")
      };
   }

   private EmbedFieldBuilder MakeFieldBuilderFromEntry(QuickEntry entry)
   {
      string fieldValue = "";

      if (entry.Hour == null)
         fieldValue += $"- **Hour:** Both\n";
      else
         fieldValue += $"- **Hour:** {entry.Hour.ToString()}\n";

      if (!string.IsNullOrEmpty(entry.TeacherId))
         fieldValue += $"- **Teacher:** {entry.TeacherId}\n";

      if (!string.IsNullOrEmpty(entry.Subject))
         fieldValue += $"- **Subject:** {entry.Subject}\n";

      if (!string.IsNullOrEmpty(entry.Description))
         fieldValue += $"- **Optional Description:** {entry.Description}\n";

      var builder = new EmbedFieldBuilder()
         .WithValue(fieldValue)
         .WithName("Options");

      return builder;
   }

   [ComponentInteraction("add_description:*")]
   public async Task HandleAddDescriptionButton(string tokenString)
   {
      if (!Guid.TryParse(tokenString, out var token) || !_quickEntryService.PendingEntries.ContainsKey(token))
      {
         await FollowupAsync("The quick entry expired or is invalid!", ephemeral: true);
         return;
      }

      await RespondWithModalAsync<AddDescriptionModal>($"add_description_modal:{tokenString}");
   }

   [ModalInteraction("add_description_modal:*")]
   public async Task HandleAddDescriptionModal(string tokenString, AddDescriptionModal modal)
   {
      if (string.IsNullOrEmpty(modal.DescriptionInput))
      {
         await FollowupAsync("Description is null!", ephemeral: true);
         return;
      }

      if (!Guid.TryParse(tokenString, out var token) || !_quickEntryService.PendingEntries.TryGetValue(token, out var entry))
      {
         await FollowupAsync("The quick entry expired or is invalid!", ephemeral: true);
         return;
      }

      entry.Description = modal.DescriptionInput;

      await HandleQuickEntry(tokenString);
   }

   [ComponentInteraction("quickentry:*")]
   public async Task HandleQuickEntry(string tokenString)
   {
      await DeferAsync(ephemeral: true);

      if (!Guid.TryParse(tokenString, out var token) || !_quickEntryService.PendingEntries.TryGetValue(token, out var entry))
      {
         await FollowupAsync("The quick entry expired or is invalid!", ephemeral: true);
         return;
      }

      var success = await MakeQuickEntry(entry);

      if (success)
         await FollowupAsync($"Successfully made entry for date {entry.Date}", ephemeral: true);
   }

   private async Task<bool> MakeQuickEntry(QuickEntry entry)
   {
      if (DateOnly.TryParse(entry.Date, out var date))
      {
         await FollowupAsync("Date parameter is not a valid IndY-Day!", ephemeral: true);
         return false;
      }

      switch (entry.Type)
      {
         case EntryType.Normal:
            return await TryMakeEntry(async () =>
                  {
                     if (entry.Hour == null)
                        await _client!.MakeNormalEntryAsync(date, entry.TeacherId, entry.Subject, entry.Description);
                     else
                        await _client!.MakeNormalEntryAsync(date, (int) entry.Hour, entry.TeacherId, entry.Subject, entry.Description);
                  });

         case EntryType.Absence:
            return await TryMakeEntry(async () =>
                  {
                     if (entry.Hour == null)
                        await _client!.MakeAbsenceEntryAsync(date);
                     else
                        await _client!.MakeAbsenceEntryAsync(date, (int) entry.Hour);
                  });

         case EntryType.Schoolevent:
            return await TryMakeEntry(async () =>
                  {
                     if (entry.Hour == null)
                        await _client!.MakeSchoolEventEntryAsync(date, entry.TeacherId, entry.Description);
                     else
                        await _client!.MakeSchoolEventEntryAsync(date, (int) entry.Hour, entry.Subject, entry.Description);
                  });

         default:
            throw new Exception("Type not recognised!");
      }
   }

   private async Task<bool> TryMakeEntry(Func<Task> action)
   {
      try {
         await action();
         return true;
      }
      catch (NotFoundException)
      {
         await FollowupAsync("No hour for this teacher on this day!", ephemeral: true);

         return false;
      }
      catch (InvalidIndyDayException)
      {
         await FollowupAsync("Not a valid IndY-Day!", ephemeral: true);

         return false;
      }
      catch (Exception)
      {
         await FollowupAsync("Something went wrong!", ephemeral: true);

         return false;
      }
   }
}
