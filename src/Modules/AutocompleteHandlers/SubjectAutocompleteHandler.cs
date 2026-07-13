using Discord;
using Discord.Interactions;
using IndYLib.Services;

namespace IndYBot.Modules.AutocompleteHandlers;

public class SubjectAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
          IInteractionContext context,
          IAutocompleteInteraction autocompleteInteraction,
          IParameterInfo parameter,
          IServiceProvider services)
    {
      string input = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

      try
      {
         var indyDays = (await IndyClient.GetActiveSubjectsAsync()).Select(x => x.SubjectId);

         var suggestion = indyDays
            .Where(x => x.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .Select(x => new AutocompleteResult(x, x))
            .Take(25);

         return AutocompletionResult.FromSuccess(suggestion);
      }
      catch (Exception e)
      {
         return AutocompletionResult.FromError(e);
      }
    }
}
