using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Reflection;

namespace IndYBot;

public class InteractionHandler
{
   private readonly DiscordSocketClient _client;
   private readonly InteractionService _handler;
   private readonly IServiceProvider _services;

   public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
   {
      _client = client;
      _handler = handler;
      _services = services;
   }

   public async Task InitAsync()
   {
      _client.Ready += ReadyAsync;
   }

   private async Task ReadyAsync()
   {
      await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

      ulong tmpGuildID = UInt64.Parse(File.ReadAllText("bot-info/tmpGuild").Trim());
      var meh = await _handler.RegisterCommandsToGuildAsync(tmpGuildID);

      Console.WriteLine($"{meh.Count()} commands are registered");

      ulong tmpChannelID = UInt64.Parse(File.ReadAllText("bot-info/tmpChannel").Trim());
      var channel = _client.GetChannel(tmpChannelID) as IMessageChannel;
      if (channel != null)
         await channel.SendMessageAsync("online!");
   }
}
