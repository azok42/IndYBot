using Discord;
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

   [RequireUserPermission(Discord.GuildPermission.Administrator)]
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

      var sql = "UPDATE guild SET default_channel = COALESCE(default_channel, @DefaultChannel), log_channel = COALESCE(log_channel, @Log), auto_entry_channel = COALESCE(auto_entry_channel, @AutoEntry), group_entry_channel = COALESCE(group_entry_channel, @GroupEntry) WHERE id = @GuildId;";
      await con.QueryAsync(sql, parameter);

      await RespondAsync("Successfully set new channels! To change channels use '/admin channelset' or to list used channels use '/admin channellist'!");
   }

   [RequireOwner]
   [SlashCommand("global-message", "Sends a message to all channels!")]
   public async Task SendGlobalMessage([Summary("message", "The message to be sent!")] string msg)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT default_channel FROM guild;";
      var channelIds = await con.QueryAsync<ulong>(sql);

      if (channelIds == null || !channelIds.Any())
      {
         await FollowupAsync("No guilds found in the database???", ephemeral: true);
         return;
      }

      foreach (var channelId in channelIds)
      {
         var channel = (await Context.Client.GetChannelAsync(channelId)) as IMessageChannel;

         if (channel == null)
         {
            await FollowupAsync($"Invalid channel ID saved in database: {channelId}", ephemeral: true);
            continue;
         }

         await channel.SendMessageAsync(msg, allowedMentions: AllowedMentions.All);
      }

      await RespondAsync("Sent message to all available guilds!", ephemeral: true);
   }
}
