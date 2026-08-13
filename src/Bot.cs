using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IndYLib.Extensions;
using IndYLib.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using IndYBot.Services;

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

      var config = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json")
         .Build();

      var masterKey = config["Database:EncryptionKey"];
      if (masterKey == null)
         throw new ArgumentNullException("MasterKey was not found!");
      SecurityHelper.Init(masterKey);

      var dbConnectionString = config["Database:Connection"];
      if (dbConnectionString == null)
         throw new ArgumentNullException("Db connection string was not found!");

      using var services = new ServiceCollection()
         .AddSingleton(config)
         .AddSingleton(socketConfig)
         .AddSingleton<DiscordSocketClient>()
         .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
         .AddSingleton<InteractionHandler>()
         .AddSingleton<LoginService>()
         .AddSingleton<QuickEntryService>()
         .AddSingleton<SQLHelper>(x => new SQLHelper(dbConnectionString))
         .AddHostedService<AutoEntryService>()
         .AddIndyAuth()
         .BuildServiceProvider();

      var client = services.GetRequiredService<DiscordSocketClient>();
      var interactionService = services.GetRequiredService<InteractionService>();
      var indyAuth = services.GetRequiredService<IIndyAuth>();

      client.Log += LogAsync;
      interactionService.Log += LogAsync;

      await services.GetRequiredService<InteractionHandler>().InitAsync();

      var token = config["Bot:Token"];
      if (token == null)
         throw new ArgumentNullException("Bot token was not found!");

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
