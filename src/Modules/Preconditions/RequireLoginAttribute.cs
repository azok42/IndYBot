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

      if (_loginService.HasClient(userId))
         return PreconditionResult.FromSuccess();

      using var con = _sqlHelper.CreateConnection();

      string sql = "SELECT name, password FROM user WHERE id = @Id;";
      var userCreds = con.QueryFirstOrDefault(sql, new {Id = userId});

      if (userCreds == null)
         return PreconditionResult.FromError("No credentials found for this user!");
      if (string.IsNullOrEmpty((string) userCreds.password))
         return PreconditionResult.FromError("No password has been set by the user!");

      await _loginService.AddClient(userId, userCreds.name, userCreds.password);

      if (_loginService.HasClient(userId))
         return PreconditionResult.FromSuccess();

      return PreconditionResult.FromError("Something went wrong at authentication!");
   }
}
