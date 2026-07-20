using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Extensions;
using Dapper;

namespace IndYBot.Modules;

public enum Standards
{
   [ChoiceDisplay("Global Teacher")]
   GlobalTeacher,
   [ChoiceDisplay("Global Subject")]
   GlobalSubject,
   [ChoiceDisplay("Global Description")]
   GlobalDescription,

   [ChoiceDisplay("Monday Teacher")]
   MondayTeacher,
   [ChoiceDisplay("Monday Subject")]
   MondaySubject,
   [ChoiceDisplay("Monday Description")]
   MondayDescription,

   [ChoiceDisplay("Wednesday Teacher")]
   WednesdayTeacher,
   [ChoiceDisplay("Wednesday Subject")]
   WednesdaySubject,
   [ChoiceDisplay("Wednesday Description")]
   WednesdayDescription,

   [ChoiceDisplay("Friday Teacher")]
   FridayTeacher,
   [ChoiceDisplay("Friday Subject")]
   FridaySubject,
   [ChoiceDisplay("Friday Description")]
   FridayDescription,
}

[Group("standard", "Commands for interacting with user-defined standa")]
public class EntryStandardsModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly SQLHelper _sqlHelper;

   private ulong UserId;

   public EntryStandardsModule(SQLHelper sqlHelper)
   {
      _sqlHelper = sqlHelper;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      UserId = Context.Interaction.User.Id;
   }

   [SlashCommand("info", "Get basic info about standards!")]
   public async Task StandardInfoCommand()
   {
      await RespondAsync("# Standards\nYou can set standards to make entry making easier and faster!\nGlobal standards are for everyday and day-specific are onl for this day! However you can always override some specific options when making the entry!\n", ephemeral: true);
   }

   [SlashCommand("list", "List all set standards!")]
   public async Task GetStandardsCommand()
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT type, value FROM user_standard WHERE id = @Id;";
      var standards = (await con.QueryAsync(sql, new { Id = UserId })).ToList();

      if (!standards.Any())
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"No standards found!");
         return;
      }

      var lines = standards.Select(s => $"- **{Enum.Parse<Standards>((string) s.type).GetChoiceDisplay()}:** {s.value}");
      var response = string.Join("\n", lines);

      await ModifyOriginalResponseAsync(x => x.Content = response);
   }

   [SlashCommand("set", "Sets a standard for your user!")]
   public async Task SetStandardsCommand(
         [Summary("standard", "The standard you want to set or change!")] Standards type,
         [Summary("value", "The wanted value for the standard!")] string value)
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();

      var sql = "INSERT INTO user_standard (id, type, value) VALUES (@Id, @Type, @VALUE) ON DUPLICATE KEY UPDATE value=@VALUE;";
      await con.QueryAsync(sql, new { Id = UserId, Type = type.ToString(), Value = value });

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully set standard '{type.GetChoiceDisplay()}' to value '{value}'");
   }

   [SlashCommand("remove", "Remove a standard from the database!")]
   public async Task RemoveStandardsCommand(
         [Summary("standard", "The standard you want to set or change!")] Standards type)
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();

      var sql = "DELETE FROM user_standard WHERE id = @Id AND type = @Type;";
      await con.QueryAsync(sql, new { Id = UserId, Type = type.ToString() });

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully delete standard '{type.GetChoiceDisplay()}'");
   }

   [SlashCommand("removeall", "Remove all of your standards from the database!")]
   public async Task DropStandardsCommand()
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();

      var sql = "DELETE FROM user_standard WHERE id = @Id;";
      await con.QueryAsync(sql, new { Id = UserId });

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully deleted all standards for your user!");
   }
}
