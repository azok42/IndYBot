using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace IndYBot;

class Bot
{
   public static Task Main(string[] args) => new Bot().MainAsync();

   public async Task MainAsync()
   {
      var socketConfig = new DiscordSocketConfig
      {
         GatewayIntents = GatewayIntents.AllUnprivileged
      };

      using var services = new ServiceCollection()
         .AddSingleton(socketConfig)
         .AddSingleton<DiscordSocketClient>()
         .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
         .AddSingleton<InteractionHandler>()
         .BuildServiceProvider();

      var client = services.GetRequiredService<DiscordSocketClient>();
      var interactionService = services.GetRequiredService<InteractionService>();

      client.Log += LogAsync;
      interactionService.Log += LogAsync;

      await services.GetRequiredService<InteractionHandler>().InitAsync();

      string token = File.ReadAllText("bot-info/token").Trim();
      await client.LoginAsync(TokenType.Bot, token);
      await client.StartAsync();

      await Task.Delay(-1);
   }

   private Task LogAsync(LogMessage log)
   {
      Console.WriteLine(log.ToString());
      return Task.CompletedTask;
   }
}
