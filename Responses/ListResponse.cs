using Discord;
using IndYBot.Responses.Interfaces;
using System.Text;

namespace IndYBot.Responses;

/// <summary>
///   Response for slash commands including a list.
/// </summary>
/// <typeparam name="T">The type of the items to be listed.</typeparam>
public class ListResponse<T>(
    IEnumerable<T> items,
    Func<T, string> format,
    string? heading = null,
    string? message = null,
    bool isEphemeral = false) : IResponse
{
    /// <summary>
    ///   The objects to send in list form.
    /// </summary>
    public IEnumerable<T> Items { get; } = items;

    /// <summary>
    ///   The format of the items to send.
    ///
    ///   <example>
    ///     Example usage:
    ///
    ///     <code>
    ///       item => $"- {item.value}: {item.value2}";
    ///     </code>
    ///   </example>
    /// </summary>
    public Func<T, string> Format { get; set; } = format;

    /// <summary>
    ///   The optional heading. Visualized with '#';
    /// </summary>
    public string? Heading { get; set; } = heading;

    /// <summary>
    ///   The optional message in addition to the list. It is put after the heading but before the list.
    /// </summary>
    public string? Message { get; set; } = message;

    /// <summary>
    ///   Wether the response is ephemeral or not.
    /// </summary>
    public bool IsEphemeral { get; set; } = isEphemeral;

    public async Task SendAsync(IInteractionContext ctx)
    {
        var output = new StringBuilder();

        if (!string.IsNullOrEmpty(Heading))
            output.AppendLine($"# {Heading}");

        if (!string.IsNullOrEmpty(Message))
            output.AppendLine($"{Message}");

        if (output.Length > 0)
            output.AppendLine();

        foreach (var item in Items)
        {
            var formatted = Format(item);

            if (output.Length + formatted.Length > IResponse.MaxMessageLength)
            {
                await ctx.Interaction.FollowupAsync(output.ToString());
                output.Clear();
            }

            output.Append(formatted);
        }

        if (output.Length > 0)
            await ctx.Interaction.FollowupAsync(output.ToString());
    }
}
