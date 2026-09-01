using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IndYBot.Bot;

public class IndyBot : IHostedService
{
   private readonly DiscordSocketClient _client;
   private readonly InteractionService _interactionService;
   private readonly InteractionHandler _interactionHandler;
   private readonly IConfiguration _config;
   private readonly ILogger<IndyBot> _logger;

   public IndyBot(
         DiscordSocketClient client,
         InteractionService interactionService,
         InteractionHandler interactionHandler,
         IConfiguration config,
         ILogger<IndyBot> logger)
   {
      _client = client;
      _interactionService = interactionService;
      _interactionHandler = interactionHandler;
      _config = config;
      _logger= logger;
   }

   public async Task StartAsync(CancellationToken cancellationToken)
   {
      _client.Log += LogAsync;
      _interactionService.Log += LogAsync;
      
      await _interactionHandler.InitAsync();

      var token = _config["Bot:Token"];
      if (token == null)
         throw new ArgumentNullException("Bot token was not found!");

      _logger.LogInformation("Logging into Discord...");

      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      _logger.LogInformation("Bot has started.");
   }

   public async Task StopAsync(CancellationToken cancellationToken)
   {
      _logger.LogInformation("Stopping Bot ...");

      await _client.StopAsync();
      await _client.LogoutAsync();

      _logger.LogInformation("Bot stopped and logged out.");
   }

    private Task LogAsync(LogMessage log)
   {
      var level = log.Severity switch
      {
         LogSeverity.Critical => LogLevel.Critical,
         LogSeverity.Error => LogLevel.Error,
         LogSeverity.Warning => LogLevel.Warning,
         LogSeverity.Info => LogLevel.Information,
         LogSeverity.Debug => LogLevel.Debug,
         LogSeverity.Verbose => LogLevel.Trace,
         _ => LogLevel.None
      };

      _logger.Log(
            level,
            log.Exception,
            "{Source} : {Message}",
            log.Source,
            log.Message);

      return Task.CompletedTask;
   }
}
