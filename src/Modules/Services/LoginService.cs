using IndYLib.Interfaces;
using IndYLib.Exceptions;

namespace IndYBot.Modules.Services;

public class ClientSession
{
   public IIndyClient Client { get; set; }
   
   public DateTime RefreshTime { get; set; }

   public ClientSession(IIndyClient Client, DateTime RefreshTime)
   {
      this.Client = Client;
      this.RefreshTime = RefreshTime;
   }
}

public class LoginService
{
   public readonly IIndyAuth _indyAuth;

   private Dictionary<ulong, ClientSession> clients = new();

   public LoginService(IIndyAuth indyAuth)
   {
      _indyAuth = indyAuth;
   }

   public void AddClient(ulong userId, IIndyClient client)
   {
      var session = new ClientSession(client, DateTime.UtcNow.AddMinutes(30));
      clients.Add(userId, session);
   }

   public async Task<IIndyClient> AddClient(ulong userId, string username, string password)
   {
      var client = await _indyAuth.CreateClientAsync(username, password);

      var session = new ClientSession(client, DateTime.UtcNow.AddMinutes(30));
      clients.Add(userId, session);

      return client;
   }

   public bool RemoveClient(ulong userId)
   {
      return clients.Remove(userId);
   }

   public IIndyClient? GetClient(ulong userId)
   {
      clients.TryGetValue(userId, out var session);

      return session == null ? null : session.Client;
   }

   public ClientSession? GetSession(ulong userId)
   {
      clients.TryGetValue(userId, out var session);

      return session;
   }

   public bool HasClient(ulong userId) => clients.ContainsKey(userId);

   /// <summary>
   /// Update the refresh time of the session for user with <paramref name="userId"/>
   /// </summary>
   /// <param name="userId">The Id of the user session to update.</param>
   /// <returns>True on success. False if no session has been found.</returns>
   public bool UpdateRefreshTime(ulong userId)
   {
      if (!HasClient(userId))
         return false;

      GetSession(userId)!.RefreshTime = DateTime.UtcNow;

      return true;
   }

   /// <summary>
   /// Checks if the client for user with <paramref name="userId"/> has a valid token or not.
   /// </summary>
   /// <param name="userId">The Id of the user to check.</param>
   /// <returns>True if the access token is still valid or false if the user does not exist in the dict or is not valid.</returns>
   public bool CheckValidClient(ulong userId)
   {
      if (!HasClient(userId))
         return false;

      var session = GetSession(userId);

      if (session!.RefreshTime.CompareTo(DateTime.UtcNow) < 0)
         return true;

      var client = session.Client;
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

      UpdateRefreshTime(userId);

      return true;
   }
}
