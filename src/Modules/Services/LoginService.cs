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

   public IIndyClient? GetClient(ulong userId)
   {
      clients.TryGetValue(userId, out var client);

      return client;
   }

   public bool HasClient(ulong userId) => clients.ContainsKey(userId);
}
