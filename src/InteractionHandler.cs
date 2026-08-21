using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using IndYBot.Helpers;
using Dapper;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace IndYBot;

public class InteractionHandler
{
   private readonly DiscordSocketClient _client;
   private readonly InteractionService _handler;
   private readonly IServiceProvider _services;
   private readonly SQLHelper _sqlHelper;
   private readonly IConfigurationRoot _config;

   private static bool commandsRegistered = false;
   private static string disconnectMsg= "";
   private static DateTime? disconnectTime = null;

   public InteractionHandler(
         DiscordSocketClient client, 
         InteractionService handler, 
         IServiceProvider services,
         SQLHelper sqlHelper,
         IConfigurationRoot config)
   {
      _client = client;
      _handler = handler;
      _services = services;
      _sqlHelper = sqlHelper;
      _config = config;
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
      if (_config["Debug:Enabled"] == "true")
      {
         Console.WriteLine("Running in DEBUG mode!");   

         var debugChannelIdString = _config["Debug:Channel"];
         if (debugChannelIdString == null)
            throw new Exception("Debug channel id was not found!");

         ulong debugChannelId = UInt64.Parse(debugChannelIdString);
         var channel = _client.GetChannel(debugChannelId) as IMessageChannel;
         if (channel != null)
            await channel.SendMessageAsync("online in debug mode!");
      }

      if (!string.IsNullOrEmpty(disconnectMsg))
      {
         await LogToGuildsAsync(disconnectMsg);
         disconnectMsg = "";
      }

      if (commandsRegistered)
         return;

      await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

      if (_config["Debug:Enabled"] == "true")
      {
         var debugGuildIdString = _config["Debug:Guild"];
         if (debugGuildIdString == null)
            throw new Exception("Debug guild id was not found!");

         ulong debugGuildId = UInt64.Parse(debugGuildIdString);
         var commands = await _handler.RegisterCommandsToGuildAsync(debugGuildId);

         Console.WriteLine($"{commands.Count()} commands have been registered");

      }
      else
      {
         await _handler.RegisterCommandsGloballyAsync();
      }

      commandsRegistered = true;
   }

   private async Task LogToGuildsAsync(string msg)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT default_channel, log_channel, logs_enabled FROM guild;";
      var guildLogSettings = await con.QueryAsync<(ulong Default, ulong Log, bool LogsEnabled)>(sql);

      if (guildLogSettings == null)
         return;
      
      foreach (var settings in guildLogSettings)
      {
         if (!settings.LogsEnabled)
            continue;

         var usedChannelId = settings.Log;

         if (settings.Log == default)
            usedChannelId = settings.Default;

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

      string msg = "";

      switch (result.Error)
      {
         case InteractionCommandError.UnmetPrecondition:
            msg = result.ErrorReason;
            break;

         case InteractionCommandError.UnknownCommand:
            msg = "Unkown command?";
            break;

         case InteractionCommandError.BadArgs:
            msg = "Invalid arguments given.";
            break;

         case InteractionCommandError.ConvertFailed:
            msg = "Invalid format for a parameter.";
            break;

         case InteractionCommandError.ParseFailed:
            msg = "Unable to parse command context.";
            break;

         case InteractionCommandError.Exception:
            msg = $"Command had an internal error: {result.ErrorReason}";

            if (result is ExecuteResult executeResult && executeResult.Exception != null)
            {
               Console.WriteLine("Exception");
               Console.WriteLine(executeResult.Exception);
               Console.WriteLine("StackTrace");
               Console.WriteLine(executeResult.Exception.StackTrace);
            }
            break;

         default:
            msg = $"Command failed: {result.ErrorReason}";
            break;
      }
      
      if (ctx.Interaction.HasResponded)
         await ctx.Interaction.FollowupAsync(msg, ephemeral: true);
      else
         await ctx.Interaction.RespondAsync(msg, ephemeral: true);
   }

   private async Task HandleNewGuild(SocketGuild guild)
   {
      var con = _sqlHelper.CreateConnection();

      var sql = "INSERT INTO guild(id, name, default_channel) VALUES(@GuildId, @Name, @DefaultChannel);";
      await con.QueryAsync(sql, new { GuildId = guild.Id, Name = guild.Name, DefaultChannel = guild.DefaultChannel.Id });

      await guild.DefaultChannel.SendMessageAsync("# Thank you for using IndYBot in your server!\nInitialize the bot the for the very first time with '/admin init' or set specific configurations with '/admin channel'!");
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
