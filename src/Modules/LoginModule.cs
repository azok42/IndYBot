using Discord.Interactions;
using IndYBot.Modules.Modals;
using IndYBot.Modules.Services;
using IndYLib.Interfaces;

namespace IndYBot.Modules;

public class LoginModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly IIndyAuth _indyAuth;
   private readonly LoginService _loginService;

   public LoginModule(IIndyAuth indyAuth, LoginService loginService)
   {
      _indyAuth = indyAuth;
      _loginService = loginService;
   }

   [SlashCommand("login", "Login temporarily without saving credentials!")]
   public async Task LoginCommand()
   {
      await RespondWithModalAsync<LoginModal>("login-modal");
   }

   [SlashCommand("student", "Get info about yourself!")]
   public async Task StudentInfoCommand()
   {
      var client = (await _loginService.getClient(Context.Interaction.User.Id).GetStudentAsync()).First();

      await RespondAsync($"{client.StudentId}: {client.Firstname} {client.Lastname} {client.Class} ({client.EMail})", ephemeral: true);
   }

   [ModalInteraction("login-modal")]
   public async Task HandleLoginModal(LoginModal modal)
   {
      if (modal.UsernameInput == null || modal.PasswordInput == null)
         throw new NullReferenceException("Input is null");
      
      _loginService.addClient(Context.Interaction.User.Id, await _indyAuth.CreateClientAsync(modal.UsernameInput, modal.PasswordInput));

      await RespondAsync("Login successful!", ephemeral: true);
   }
}
