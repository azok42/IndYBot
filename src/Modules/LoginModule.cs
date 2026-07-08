using Discord.Interactions;
using IndYBot.Modules.Modals;
using IndYBot.Modules.Services;
using IndYBot.Helpers;
using Dapper;

namespace IndYBot.Modules;

public class LoginModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;
   private readonly SQLHelper _sqlHelper;

   public LoginModule(LoginService loginService, SQLHelper sqlHelper)
   {
      _loginService = loginService;
      _sqlHelper = sqlHelper;
   }

   [SlashCommand("login", "Login temporarily without saving credentials!")]
   public async Task LoginCommand()
   {
      if (_loginService.HasClient(Context.Interaction.User.Id))
      {
         await RespondAsync("No need to login! Already have you in the system!", ephemeral: true);
         return;
      }

      var con = _sqlHelper.CreateConnection();

      var sql = "SELECT name FROM user WHERE id = @Id LIMIT 1;";
      var username = await con.QueryFirstAsync(sql, new { Id = Context.Interaction.User.Id });

      await RespondWithModalAsync<LoginModal>("login-modal", new LoginModal(username.name));
   }

   [ModalInteraction("login-modal")]
   public async Task HandleLoginModal(LoginModal modal)
   {
      if (modal.UsernameInput == null || modal.PasswordInput == null)
         throw new NullReferenceException("Input is null");
 
      await _loginService.AddClient(Context.Interaction.User.Id, modal.UsernameInput, modal.PasswordInput);

      await RespondAsync("Login successful!", ephemeral: true);
   }

   [SlashCommand("save", "Save username and password for later logins!")]
   public async Task SaveLoginCommand()
   {
      await RespondWithModalAsync<SaveLoginModal>("savelogin-modal");
   }

   [ModalInteraction("savelogin-modal")]
   public async Task HandleSaveLoginModal(SaveLoginModal modal)
   {
      if (modal.UsernameInput == null)
         throw new NullReferenceException("Input is null");

      var con = _sqlHelper.CreateConnection();

      var sql = "INSERT INTO user (id, name, password) VALUES (@Id, @Name, @Password) ON DUPLICATE KEY UPDATE name=@Name, password=@Password;";
      await con.QueryAsync(sql, new {Id = Context.Interaction.User.Id, Name = modal.UsernameInput, Password = modal.PasswordInput});

      await RespondAsync("Please save successful! Make a '/info student call' to ensure it worked!", ephemeral: true);
   }
}
