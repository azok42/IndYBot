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

   [SlashCommand("specialindy", "Get a list of current special indys!")]
   public async Task SpecialIndyCommand()
   {
      var specialIndys = await IndyClient.GetSpecialIndyAsync();

      await RespondAsync("Getting all special indys...", ephemeral: true);

      bool sendExtraMsg = false;
      string msg = "# Special Indy:\n";
      foreach (var specialIndy in specialIndys)
      {
         string tmpMsg = $"- {specialIndy}\n";

         if (msg.Count() + tmpMsg.Count() >= 2000)
         {
            await Context.Channel.SendMessageAsync(msg);
            msg = tmpMsg;
            sendExtraMsg = false;
         }
         else
         {
            msg += tmpMsg;
            sendExtraMsg = true;
         }
      }

      if (sendExtraMsg)
         await Context.Channel.SendMessageAsync(msg);
   }
}
