using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Modals;
using Dapper;

namespace IndYBot.Modules;

[Group("admin", "Various admin commands!")]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly SQLHelper _sqlHelper;

   public AdminModule(SQLHelper sqlHelper)
   {
      _sqlHelper = sqlHelper;
   }

   [SlashCommand("initialize", "Sets up your server for the very first time!")]
   public async Task InitCommand()
   {
      await RespondWithModalAsync<InitModal>("init");
   }

   [ModalInteraction("init", ignoreGroupNames: true)]
   public async Task HandleInitModal(InitModal modal)
   {
      var con = _sqlHelper.CreateConnection();

      ulong defaultChannel = 0;
      if (modal.DefaultChannel != null && modal.DefaultChannel.Any())
         defaultChannel = modal.DefaultChannel.First().Id;

      ulong logChannel = 0;
      if (modal.LogChannel != null && modal.LogChannel.Any())
         logChannel = modal.LogChannel.First().Id;

      ulong autoEntryChannel = 0;
      if (modal.AutoEntryChannel != null && modal.AutoEntryChannel.Any())
         autoEntryChannel = modal.AutoEntryChannel.First().Id;

      ulong groupEntryChannel = 0;
      if (modal.GroupEntryChannel != null && modal.GroupEntryChannel.Any())
         groupEntryChannel = modal.GroupEntryChannel.First().Id;

      var parameter = new {
         DefaultChannel = defaultChannel,
         Log = logChannel,
         AutoEntry = autoEntryChannel,
         GroupEntry = groupEntryChannel,
         GuildId = Context.Guild.Id
      };

      var sql = "UPDATE guild SET default_channel = COALESCE(default_channel, @DefaultChannel), log = COALESCE(log, @Log), auto_entry = COALESCE(auto_entry, @AutoEntry), group_entry = COALESCE(group_entry, @GroupEntry) WHERE id = @GuildId;";
      await con.QueryAsync(sql, parameter);

      await RespondAsync("Successfully set new channels! To change channels use '/admin channelset' or to list used channels use '/admin channellist'!");
   }
}
