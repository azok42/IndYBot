using Discord;
using Discord.Interactions;
using IndYBot.Modules.Preconditions;
using IndYBot.Modules.AutocompleteHandlers;
using IndYBot.Modules.Services;
using IndYLib.Interfaces;

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

      var component = new ComponentBuilder()
         .WithButton("Make entry", $"quickentry:{entryToken}", ButtonStyle.Success)
         .Build();

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
               x.Components = component;
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

   [ComponentInteraction("quickentry:*")]
   public async Task HandleQuickEntry(string tokenString)
   {
      if (!Guid.TryParse(tokenString, out var token) || !_quickEntryService.PendingEntries.TryRemove(token, out var value))
      {
         await RespondAsync("The quick entry expired or is invalid!", ephemeral: true);
         return;
      }

      await RespondAsync("works (holy shit)", ephemeral: true);
   }
}
