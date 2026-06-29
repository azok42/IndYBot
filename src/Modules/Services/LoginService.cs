using IndYLib.Interfaces;

namespace IndYBot.Modules.Services;

public class LoginService
{
   public Dictionary<ulong, IIndyClient> clients = new();

   public void AddClient(ulong userId, IIndyClient client)
   {
      clients.Add(userId, client);
   }

   public IIndyClient GetClient(ulong userId)
   {
      return clients[userId];
   }
}
