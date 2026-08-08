using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using IndYBot.Helpers;
using Dapper;
using System.Reflection;

namespace IndYBot;

public class InteractionHandler
{
   private readonly DiscordSocketClient _client;
   private readonly InteractionService _handler;
   private readonly IServiceProvider _services;
   private readonly SQLHelper _sqlHelper;

   private static bool commandsRegistered = false;
   private static string disconnectMsg= "";
   private static DateTime? disconnectTime = null;

   public InteractionHandler(
         DiscordSocketClient client, 
         InteractionService handler, 
         IServiceProvider services,
         SQLHelper sqlHelper)
   {
      _client = client;
      _handler = handler;
      _services = services;
      _sqlHelper = sqlHelper;
   }

   public async Task InitAsync()
   {
      _client.Ready += ReadyAsync;
      _client.InteractionCreated += HandleInteractionAsync;
      _client.JoinedGuild += HandleNewGuild;
      _client.LeftGuild += HandleGuildLeft;
      _client.Disconnected += HandleDisconnect;

      _handler.InteractionExecuted += HandleInteractionExecutedAsync;
   }

   private async Task ReadyAsync()
   {
      #if DEBUG
         Console.WriteLine("Running in DEBUG mode!");   

         ulong debugChannelId = UInt64.Parse(File.ReadAllText("bot-info/debugChannel").Trim());
         var channel = _client.GetChannel(debugChannelId) as IMessageChannel;
         if (channel != null)
            await channel.SendMessageAsync("online in debug mode!");
      #endif

      if (!string.IsNullOrEmpty(disconnectMsg))
      {
         await LogToGuildsAsync(disconnectMsg);
         disconnectMsg = "";
      }

      if (commandsRegistered)
         return;

      await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

      #if DEBUG

         ulong debugGuildId = UInt64.Parse(File.ReadAllText("bot-info/debugGuild").Trim());
         var commands = await _handler.RegisterCommandsToGuildAsync(debugGuildId);

         Console.WriteLine($"{commands.Count()} commands have been registered");

      #else

         await _handler.RegisterCommandsGloballyAsync();

      #endif

      commandsRegistered = true;
   }

   private async Task LogToGuildsAsync(string msg)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT default_channel, log_channel FROM guild;";
      var channels = await con.QueryAsync<(ulong Default, ulong Log)>(sql);

      if (channels == null)
         return;
      
      foreach (var channelPair in channels)
      {
         var usedChannelId = channelPair.Log;

         if (channelPair.Log == default)
            usedChannelId = channelPair.Default;

         var usedChannel = await _client.GetChannelAsync(usedChannelId) as IMessageChannel;

         if (usedChannel != null)
            await usedChannel.SendMessageAsync($"[LOG] Channel disconnected at {disconnectTime} and reconnected now! Message: {disconnectMsg}"); }
   }

   private async Task HandleInteractionAsync(SocketInteraction interaction)
   {
      try
      {
         var ctx = new SocketInteractionContext(_client, interaction);
         await _handler.ExecuteCommandAsync(ctx, _services);
      }
      catch (Exception e)
      {
         Console.WriteLine($"Error while processing interaction: {e}");

         if (interaction.Type == InteractionType.ApplicationCommand && !interaction.HasResponded)
         {
            await interaction.RespondAsync($"Error while processing interaction: {e.GetBaseException()}");
         }
      }
   }

   private async Task HandleInteractionExecutedAsync(ICommandInfo command, IInteractionContext ctx, IResult result)
   {
      if (result.IsSuccess)
         return;

      switch (result.Error)
      {
         case InteractionCommandError.UnmetPrecondition:
            await ctx.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
            break;

         case InteractionCommandError.UnknownCommand:
            await ctx.Interaction.RespondAsync("Unkown command?", ephemeral: true);
            break;

         case InteractionCommandError.BadArgs:
            await ctx.Interaction.RespondAsync($"Invalid arguments given.", ephemeral: true);
            break;

         case InteractionCommandError.ConvertFailed:
            await ctx.Interaction.RespondAsync($"Invalid format for a parameter.", ephemeral: true);
            break;

         case InteractionCommandError.ParseFailed:
            await ctx.Interaction.RespondAsync($"Unable to parse command context.", ephemeral: true);
            break;

         case InteractionCommandError.Exception:
            await ctx.Interaction.RespondAsync($"Command had an internal error: {result.ErrorReason}", ephemeral: true);
            break;

         default:
            await ctx.Interaction.RespondAsync($"Command failed: {result.ErrorReason}", ephemeral: true);
            break;
      }
   }

   private async Task HandleNewGuild(SocketGuild guild)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "INSERT INTO guild(id, name, default_channel) VALUES(@GuildId, @Name, @DefaultChannel);";
      await con.QueryAsync(sql, new { GuildId = guild.Id, Name = guild.Name, DefaultChannel = guild.DefaultChannel.Id });

      await guild.DefaultChannel.SendMessageAsync("Initialize the bot the for the very first time with '/admin init' or set specific configurations with '/admin channel'!");
   }

   private async Task HandleGuildLeft(SocketGuild guild)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "DELETE FROM guild WHERE id = @GuildId;";
      await con.QueryAsync(sql, new { GuildId = guild.Id });
   }

   private async Task HandleDisconnect(Exception e)
   {
      disconnectMsg = e.Message;
      disconnectTime = DateTime.Now;
   }
}
