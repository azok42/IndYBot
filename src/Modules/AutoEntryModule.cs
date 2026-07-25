using Discord.Interactions;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using IndYLib.Interfaces;
using Dapper;

namespace IndYBot.Modules;

public enum AutoEntryStatus
{
   Enabled,
   Disabled,
   InProgress,
   Failed
}

[Group("autoentry", "Commands to manage automatic entries!")]
public class AutoEntryModule : InteractionModuleBase<SocketInteractionContext>
{
   private IIndyClient? _client;
   private readonly LoginService _loginService;
   private readonly SQLHelper _sqlHelper;

   public AutoEntryModule(LoginService loginService, SQLHelper sqlHelper)
   {
      _loginService = loginService;
      _sqlHelper = sqlHelper;
   }

   public override void BeforeExecute(ICommandInfo command)
   {
      _client = _loginService.GetClient(Context.Interaction.User.Id);
   }

   [SlashCommand("toggle", "Toggle automatic entries!")]
   public async Task ToggleCommand()
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT status FROM auto_entry WHERE id = @Id;";
      var statusString = await con.QueryFirstOrDefaultAsync<string>(sql, new { Id = Context.Interaction.User.Id });

      if (string.IsNullOrEmpty(statusString))
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"You first need to set a time with the '/autoentry set' command!");
         return;
      }

      var status = Enum.Parse<AutoEntryStatus>(statusString);
      AutoEntryStatus newStatus = AutoEntryStatus.Enabled;

      if (status == AutoEntryStatus.InProgress)
      {
         await ModifyOriginalResponseAsync(x => x.Content = $"Please retry in (hopefully) a few seconds. An entry is currently in progress!");
         return;
      }

      if (status == AutoEntryStatus.Enabled)
         newStatus = AutoEntryStatus.Disabled;

      var updateSql = "UPDATE auto_entry SET status = @Status WHERE id = @Id;";
      await con.QueryAsync(updateSql, new { Status = newStatus.ToString(), Id = Context.Interaction.User.Id });

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully toggled automatic entries from {statusString} to {newStatus.ToString()}!");
   }

   [SlashCommand("set", "Sets a time where automatic entries will be made!")]
   public async Task SetCommand(
         [Summary("time", "The time of the entry making!")] DateTime time)
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();
      var sql = "INSERT INTO auto_entry (id, time, status) VALUES (@Id, @Time, 'Disabled') ON DUPLICATE KEY UPDATE time=@Time, status='Disabled';";
      await con.QueryAsync(sql, new { Id = Context.Interaction.User.Id, Time = time.TimeOfDay });

      await ModifyOriginalResponseAsync(x => x.Content = $"Successfully set auto-entry time to {time.ToString("HH:mm")}");
   }
}
