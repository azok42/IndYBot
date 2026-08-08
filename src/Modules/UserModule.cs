using Discord;
using Discord.Interactions;
using IndYBot.Helpers;
using IndYBot.Modules.Services;
using IndYBot.Modules.Preconditions;
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
