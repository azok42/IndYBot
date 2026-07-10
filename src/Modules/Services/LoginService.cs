using IndYLib.Interfaces;
using IndYLib.Exceptions;
using System.Threading;

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

   /// <summary>
   /// Checks if the client for user with <paramref name="userId"/> has a valid token or not.
   /// </summary>
   /// <param name="userId">The Id of the user to check.</param>
   /// <returns>True if the access token is still valid or false if the user does not exist in the dict or is not valid.</returns>
   public bool CheckValidClient(ulong userId)
   {
      if (!HasClient(userId))
         return false;

      var client = GetClient(userId);
      try
      {
         var student = client!.GetStudentAsync();
         while (true)
         {
            if (student.IsCompletedSuccessfully)
               break;
            if (student.IsFaulted)
               throw student.Exception;
            Thread.Sleep(10);
         }
      }
      catch (InvalidTokenExcpetion)
      {
         return false;
      }

      return true;
   }
}
