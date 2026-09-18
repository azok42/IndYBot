using Discord.Interactions;
using IndYBot.Services;

namespace IndYBot.Commands;

[Group("get", "Various getter commands!")]
public class GetModule(GetService getService)
    : InteractionModuleBase
{
    private readonly GetService _getService = getService;

    [SlashCommand("subjects", "Get all available subjects!")]
    public async Task GetSubjectsCommand()
    {
        await DeferAsync();

        var response = await _getService.GetSubjects();

        await response.SendAsync(Context);
    }

    [SlashCommand("indydays", "Get the next IndY-Days!")]
    public async Task GetIndYDays(
            [Summary("month", "Optional month to get IndY-Days for!")] int month = -1)
    {
        await DeferAsync();

        var response = await _getService.GetIndyDays(month);

        await response.SendAsync(Context);
    }

    [SlashCommand("hours", "Get all available IndY-Hours!")]
    public async Task GetIndYHours()
    {
        await DeferAsync();

        var response = await _getService.GetIndyHours();

        await response.SendAsync(Context);
    }
}
