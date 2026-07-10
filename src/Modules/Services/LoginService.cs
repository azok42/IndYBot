using IndYLib.Interfaces;

namespace IndYBot.Modules.Services;

public class LoginService
{
   public readonly IIndyAuth _indyAuth;

   private Dictionary<ulong, IIndyClient> clients = new();

   public LoginService(IIndyAuth indyAuth)
   {
      _indyAuth = indyAuth;
   }

   public void AddClient(ulong userId, IIndyClient client)
   {
      clients.Add(userId, client);
   }

   public async Task<IIndyClient> AddClient(ulong userId, string username, string password)
   {
      var client = await _indyAuth.CreateClientAsync(username, password);
      clients.Add(userId, client);

      return client;
   }

   public bool RemoveClient(ulong userId)
   {
      return clients.Remove(userId);
   }

   public IIndyClient? GetClient(ulong userId)
   {
      clients.TryGetValue(userId, out var client);

      return client;
   }

   public bool HasClient(ulong userId) => clients.ContainsKey(userId);
}
