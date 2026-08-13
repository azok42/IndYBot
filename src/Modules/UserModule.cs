using Discord;
using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;
using IndYLib.Interfaces;
using IndYLib.Services;
using IndYLib.Models.Entry;
using Dapper;

namespace IndYBot.Modules;

public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;
   private readonly SQLHelper _sqlHelper;

   public UserModule(LoginService loginService, SQLHelper sqlHelper)
   {
      _loginService = loginService;
      _sqlHelper = sqlHelper;
   }

   [RequireLogin]
   [SlashCommand("setnickname", "Set your server nickname to you name on IndY!")]
   public async Task SetNicknameCommand()
   {
      var client = _loginService.GetClient(Context.Interaction.User.Id);
      var student = await client!.GetStudentAsync();

      var user = Context.Interaction.User as IGuildUser;
      await user!.ModifyAsync(x => x.Nickname = student.First().Firstname + " " + student.First().Lastname);

      await RespondAsync("Successfully changed nickname!");
   }

   // TODO: optinal day param
   [SlashCommand("whereis", "Check the entry of the specified user!")]
   public async Task WhereIsCommand([Summary("user", "The user to be checked")] IUser user)
   {
      await DeferAsync(ephemeral: true);

      var con = _sqlHelper.CreateConnection();   

      var sql = "SELECT name, password, whereis_status FROM user WHERE id = @Id;";
      var queryResult = await con.QueryFirstOrDefaultAsync<(string Username, string Password, string Status)>(sql, new { Id = user.Id });

      if (queryResult == default)
      {
         await ModifyOriginalResponseAsync(x => x.Content = "The specified user hasn't saved his credentials and can therefore be not checked!");
         return;
      }

      if (!queryResult.Status.Equals("enabled"))
      {
         await ModifyOriginalResponseAsync(x => x.Content = "The specified user has the WhereIs-feature disabled. Try remind them to enable it!");
         return;
      }

      var today = DateOnly.FromDateTime(DateTime.Today);
      var indyDays = await IndyClient.GetIndyDaysAsync(today, today.AddDays(7));

      if (!indyDays.Any())
      {
         await ModifyOriginalResponseAsync(x => x.Content = "No IndY-Day found! Try it again with setting a date yourself!");
         return;
      }

      var indyDayDate = indyDays.First().Date;

      var password = SecurityHelper.Decrypt(queryResult.Password);
      IIndyClient client;

      if (_loginService.HasClient(user.Id))
         client = _loginService.GetClient(user.Id)!;
      else
         client = await _loginService.AddClient(user.Id, queryResult.Username, password);

      var name = user.Username;
      var entries = await client.GetEntriesAsync(indyDayDate);

      var msg = GetMessageFromEntries(entries, name);
      
      await ModifyOriginalResponseAsync(x => x.Content = msg);
   }

   private string GetMessageFromEntries(FullRetured entries, string name)
   {
      var hour3 = entries.Hour3.FirstOrDefault();
      var hour4 = entries.Hour4.FirstOrDefault();

      if (hour3 == null || hour4 == null)
         return $"User {name} hasn't made entries!";
      else
      {
         var teacher3 = hour3.TeacherId;
         var teacher4 = hour4.TeacherId;

         if (hour3 != null && hour4 != null && teacher3.Equals(teacher4))
            return $"User {name} is at teacher {teacher3}!";

         else if (hour3 != null && hour4 != null && !teacher3.Equals(teacher4))
            return $"User {name} is at teacher:\n- Hour 3: {teacher3}\n-Hour 4: {teacher4}";

         else if (hour3 != null)
            return $"User {name} is at teacher:\n- Hour 3: {teacher3}\n-Hour 4: Missing entry";

         else if (hour4 != null)
            return $"User {name} is at teacher:\n- Hour 3: Missing Entry\n-Hour 4: {teacher4}";

         else
            return "There was a problem at proccesing hours!";
      }

   }

   [SlashCommand("whereis-toogle", "Toogle the WhereIs-feature for your account!")]
   public async Task ToogleWhereIsCommand()
   {
      var con = _sqlHelper.CreateConnection();     
      var userId = Context.Interaction.User.Id;

      var selectSql = "SELECT whereis_status FROM user WHERE id = @Id;";
      var currentStatus = await con.QueryFirstOrDefaultAsync<string>(selectSql, new { Id = userId });

      if (currentStatus == default)
      {
         await RespondAsync(ephemeral: true, text: "Please set your credentials first with '/save'");
         return;
      }

      string status = currentStatus.Equals("enabled") ? "disabled" : "enabled";

      var setSql = "UPDATE user SET whereis_status = @Status WHERE id = @Id;";
      await con.QueryAsync(setSql, new { Id = userId, Status = status});

      await RespondAsync(ephemeral: true, text: $"Successfully toggled WhereIs-feature to '{status}'! If enabled, other users can now see your entries for the next IndY-Day");
   }
}
