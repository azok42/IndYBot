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
}
