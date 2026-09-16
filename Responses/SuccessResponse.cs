using Discord;
using IndYBot.Responses.Interfaces;

namespace IndYBot.Responses;

public class SuccessResponse(
    string message,
    bool isEphemeral = true) : IResponse
{
    public string Message { get; } = message;
    public bool IsEphemeral { get; } = isEphemeral;

    public async Task SendAsync(IInteractionContext ctx)
    {
        if (ctx.Interaction.HasResponded)
            await ctx.Interaction.ModifyOriginalResponseAsync(
                    response => response.Content = Message);
        else
            await ctx.Interaction.RespondAsync(Message, ephemeral: IsEphemeral);
    }
}
