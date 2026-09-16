using Discord.Interactions;

using IndYBot.Responses;

namespace IndYBot.Commands;

public class TestModule : InteractionModuleBase
{
    [SlashCommand("ping", "Ping the bot!")]
    public async Task PingCommand()
    {
        var response = new SuccessResponse("Pong!", true);

        await response.SendAsync(Context);
    }
}
