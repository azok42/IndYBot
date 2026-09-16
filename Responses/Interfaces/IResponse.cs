using Discord;

namespace IndYBot.Responses.Interfaces;

public interface IResponse
{
    public const int MaxMessageLength = 2000;

    /// <summary>
    ///   Sends this response asynchronously.
    /// </summary>
    /// <param name="ctx">The current interaction context to send the response to.</param>
    /// <returns>A task representing the asynchronous send opration.</returns>
    public Task SendAsync(IInteractionContext ctx);
}
