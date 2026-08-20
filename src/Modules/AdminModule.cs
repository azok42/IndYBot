using Discord;
using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Modals;
using Dapper;

namespace IndYBot.Modules;

public enum GuildChannelType
{
   [ChoiceDisplay("Default channel")]
   DefaultChannel,

   [ChoiceDisplay("Log channel")]
   LogChannel,

   [ChoiceDisplay("Group-Entry channel")]
   GroupEntryChannel,

   [ChoiceDisplay("Auto-Entry channel")]
   AutoEntryChannel
}

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

   [RequireUserPermission(Discord.GuildPermission.Administrator)]
   [SlashCommand("disable-logs", "Disable logging for your guild!")]
   public async Task DisableLoggingCommand()
   {
      await SetLoggingStatus(false);

      await RespondAsync("Successfully disabled logging for your guild!", ephemeral: true);
   }

   [RequireUserPermission(Discord.GuildPermission.Administrator)]
   [SlashCommand("enable-logs", "Enable logging for your guild! Why did you disable it in the first place?")]
   public async Task EnableLoggingCommand()
   {
      await SetLoggingStatus(true);

      await RespondAsync("Successfully re-enabled logging for your guild!", ephemeral: true);
   }

   private async Task SetLoggingStatus(bool status)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "UPDATE guild SET logs_enabled = @Status WHERE id = @Id;";
      await con.QueryAsync(sql, new { Status = status, Id = Context.Guild.Id });
   }

   [RequireOwner]
   [SlashCommand("global-message", "Sends a message to all channels!")]
   public async Task SendGlobalMessage(
         [Summary("message", "The message to be sent!")] string msg,
         [Summary("channel", "The channel the message will be sent to! If not set by the guild, defaults back to default channel!")] 
         GuildChannelType channelType = GuildChannelType.DefaultChannel)
   {
      await DeferAsync(ephemeral: true);
      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT default_channel, log_channel, auto_entry_channel, group_entry_channel FROM guild;";
      var guildChannels = await con.QueryAsync<(ulong, ulong, ulong, ulong)>(sql);

      if (guildChannels == null || !guildChannels.Any())
      {
         await FollowupAsync("No guilds found in the database???", ephemeral: true);
         return;
      }

      foreach (var channels in guildChannels)
      {
         try
         {
            IMessageChannel channel = GetChannel(channelType, channels);
            
            await channel.SendMessageAsync(msg, allowedMentions: AllowedMentions.All);
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Failed to send message to a server: {ex.Message}");
            
            await FollowupAsync("Could not send message to one of the servers. Skipping...", ephemeral: true);
            continue;
         }
      }

      await ModifyOriginalResponseAsync(x => x.Content = "Sent message to all available guilds!");
   }

   private IMessageChannel GetChannel(GuildChannelType channelType, (ulong, ulong, ulong, ulong) channels)
   {
      var channelId = channelType switch
      {
         GuildChannelType.DefaultChannel => channels.Item1,
         GuildChannelType.LogChannel=> channels.Item2,
         GuildChannelType.AutoEntryChannel => channels.Item3,
         GuildChannelType.GroupEntryChannel => channels.Item4,
         _ => channels.Item1
      };

      if (channelId == default)
      {
         if (channelType != GuildChannelType.DefaultChannel)
            return GetChannel(GuildChannelType.DefaultChannel, channels);
         else
            throw new Exception("Requested id is default");
      }

      var channel = Context.Client.GetChannel(channelId) as IMessageChannel;

      if (channel != null)
         return channel;

      if (channelType != GuildChannelType.DefaultChannel)
         return GetChannel(GuildChannelType.DefaultChannel, channels);
      else
         throw new Exception("Invalid Id");
   }
}
