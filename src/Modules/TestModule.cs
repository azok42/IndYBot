using Discord.Interactions;

namespace IndYBot.Modules;

public class TestModule : InteractionModuleBase<SocketInteractionContext>
{
   [SlashCommand("ping", "Ping!")]
   public async Task PingCommand()
   {
      await RespondAsync("Pong!", ephemeral: true);
   }
}
