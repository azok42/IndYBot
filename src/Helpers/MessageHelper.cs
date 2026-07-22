using Discord.Interactions;

namespace IndYBot.Helpers;

public class MessageHelper
{
   public static async Task SendListMessageAsync<T>(List<T> list, SocketInteractionContext context, Func<T, string> msgListItem)
   {
      bool sendExtraMsg = false;
      string msg = "";
      foreach (var element in list)
      {
         string tmpMsg = msgListItem(element);

         if (msg.Count() + tmpMsg.Count() >= 2000)
         {
            await context.Interaction.FollowupAsync(msg);
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
         await context.Interaction.FollowupAsync(msg);
   }
}
