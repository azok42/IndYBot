using Discord;
using Discord.Interactions;
using IndYLib.Services;

namespace IndYBot.Modules.AutocompleteHandlers;

public class IndyDayAutocompleteHandler : AutocompleteHandler
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
         var today = DateOnly.FromDateTime(DateTime.Today);

         var indyDays = await IndyClient.GetIndyDaysAsync(today.AddDays(-7), today.AddDays(7));

         var suggestion = indyDays
            .Where(x => x.Date.ToString().Contains(input))
            .Distinct()
            .Select(x => new AutocompleteResult($"{x.DayName} {x.Date.ToString()}", x.Date.ToString()))
            .Take(25);

         return AutocompletionResult.FromSuccess(suggestion);
      }
      catch (Exception e)
      {
         return AutocompletionResult.FromError(e);
      }
   }
}
