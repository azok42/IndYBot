using Discord;
using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Services;
using Microsoft.Extensions.DependencyInjection;
using Dapper;

namespace IndYBot.Modules.Preconditions;

public class RequireLoginAttribute : PreconditionAttribute
{
   public override async Task<PreconditionResult> CheckRequirementsAsync(
         IInteractionContext context,
         ICommandInfo commandInfo,
         IServiceProvider services)
   {
      var _sqlHelper = services.GetService<SQLHelper>(); 
      var _loginService = services.GetService<LoginService>(); 
      if (_sqlHelper == null || _loginService == null)
         return PreconditionResult.FromError("Could not get internal classes!");

      var userId = context.Interaction.User.Id;

      if (_loginService.CheckValidClient(userId))
      {
         return PreconditionResult.FromSuccess();
      }
      else if (_loginService.HasClient(userId))
      {
         _loginService.RemoveClient(userId); // remove to not crash on re-creation
      }

      using var con = _sqlHelper.CreateConnection();

      string sql = "SELECT name, password FROM user WHERE id = @Id;";
      var userCreds = await con.QueryFirstOrDefaultAsync<(string Name, string Password)>(sql, new { Id = userId });

      if (userCreds == default)
         return PreconditionResult.FromError("**[ERROR] Login needed!** No credentials found for your user!");
      if (string.IsNullOrEmpty((string) userCreds.Password))
         return PreconditionResult.FromError("**[ERROR] Login needed!** No password has been set!");

      await _loginService.AddClient(userId, userCreds.Name, userCreds.Password);

      if (_loginService.HasClient(userId))
         return PreconditionResult.FromSuccess();

      return PreconditionResult.FromError("**[ERROR] Manual login needed!** Something went wrong at authentication!");
   }
}
