using Discord.Interactions;

using IndYBot.Services;
using IndYBot.Components.Autocomplete;

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
    public async Task GetIndYHours(
            [Autocomplete(typeof(TeacherAutocompleteHandler))] 
            [Summary("Teacher", "Filter by teacher!")] string teacher = "",
            [Summary("Day", "Filter by the day of the week")] Day? day = null,
            [Summary("Hour", "Filter by IndY-Hour")] Hour? hour = null)
    {
        await DeferAsync();

        var response = await _getService.GetIndyHours(teacher, day, hour);

        await response.SendAsync(Context);
    }
}
