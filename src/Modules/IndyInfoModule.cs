using Discord.Interactions;
using IndYLib.Services;

namespace IndYBot;

public class IndyInfoModule : InteractionModuleBase<SocketInteractionContext>
{
   [SlashCommand("subjects", "Get a list of all valid subjects!")]
   public async Task SubjectsCommand()
   {
      var subjects = await IndyClient.GetActiveSubjectsAsync();

      await RespondAsync("Getting all subjects...", ephemeral: true);

      string msg = "# Subjects:\n";
      foreach (var subject in subjects)
      {
         if (msg.Count() < 1900)
         {
            msg += $"- {subject.SubjectId} ({subject.SubjectLong})\n";
            continue;
         }

         await Context.Channel.SendMessageAsync(msg);
      }

      await Context.Channel.SendMessageAsync(msg);
   }
}
