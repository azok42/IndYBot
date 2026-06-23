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
         .BuildServiceProvider();

      var client = services.GetRequiredService<DiscordSocketClient>();
      var interactionService = services.GetRequiredService<InteractionService>();

      string token = File.ReadAllText("bot-info/token").Trim();
      await client.LoginAsync(TokenType.Bot, token);
      await client.StartAsync();

      await Task.Delay(9999);

      var channel = client.GetChannel(1493851566854115400) as IMessageChannel;
      if (channel != null)
      {
         await channel.SendMessageAsync("Test");
      }

      await Task.Delay(-1);
   }
}
