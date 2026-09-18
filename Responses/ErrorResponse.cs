using Discord;
using IndYBot.Responses.Interfaces;
using Microsoft.Extensions.Logging;

namespace IndYBot.Responses;

public class ErrorResponse(
    string message,
    LogLevel level,
    bool isEphemeral = true) : IResponse
{
    public string Message { get; } = message;
    public LogLevel Level { get; } = level;
    public bool IsEphemeral { get; } = isEphemeral;

    public async Task SendAsync(IInteractionContext ctx)
    {
        var message = FormatErrorMessage();

        if (ctx.Interaction.HasResponded)
            await ctx.Interaction.ModifyOriginalResponseAsync(
                    response => response.Content = message);
        else
            await ctx.Interaction.RespondAsync(message, ephemeral: IsEphemeral);
    }

    private string FormatErrorMessage()
    {
        var message = $"[{Level}] {Message}";

        return message;
    }
}
