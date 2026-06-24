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
      _client.InteractionCreated += HandleInteractionAsync;

      _handler.InteractionExecuted += HandleInteractionExecutedAsync;
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
            await ctx.Interaction.RespondAsync("Not enough permissions!", ephemeral: true);
            break;

         case InteractionCommandError.UnknownCommand:
            await ctx.Interaction.RespondAsync("Unkown command", ephemeral: true);
            break;

         default:
            await ctx.Interaction.RespondAsync($"Command failed: {result.ErrorReason}", ephemeral: true);
            break;
      }
   }
}
