using Discord;
using Discord.Interactions;
using IndYLib.Services;

namespace IndYBot.Components.Autocomplete;

public class TeacherAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction interaction,
            IParameterInfo parameter,
            IServiceProvider services)
    {
        var input = interaction.Data.Current.Value?.ToString() ?? "";

        try
        {
            var specialIndys = await IndyClient.GetIndyHoursAsync();
            var teacherIds = specialIndys.Select(teacher => teacher.TeacherId).ToList();

            var suggestions = teacherIds
                .Where(id => id.Contains(input, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Select(id => new AutocompleteResult(id, id))
                .Take(25);

            return AutocompletionResult.FromSuccess(suggestions);
        }
        catch (Exception e)
        {
            return AutocompletionResult.FromError(e);
        }
    }
}
