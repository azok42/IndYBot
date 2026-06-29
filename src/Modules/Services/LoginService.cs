using IndYLib.Interfaces;

namespace IndYBot.Modules.Services;

public class LoginService
{
   public Dictionary<ulong, IIndyClient> clients = new();

   public void addClient(ulong userId, IIndyClient client)
   {
      clients.Add(userId, client);
   }

   public IIndyClient getClient(ulong userId)
   {
      return clients[userId];
   }
}
