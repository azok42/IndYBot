using Discord.Interactions;
using IndYBot.Modules.Modals;
using IndYBot.Modules.Services;

namespace IndYBot.Modules;

public class LoginModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly LoginService _loginService;

   public LoginModule(LoginService loginService)
   {
      _loginService = loginService;
   }

   [SlashCommand("login", "Login temporarily without saving credentials!")]
   public async Task LoginCommand()
   {
      await RespondWithModalAsync<LoginModal>("login-modal");
   }

   [ModalInteraction("login-modal")]
   public async Task HandleLoginModal(LoginModal modal)
   {
      if (modal.UsernameInput == null || modal.PasswordInput == null)
         throw new NullReferenceException("Input is null");
      
      await _loginService.AddClient(Context.Interaction.User.Id, modal.UsernameInput, modal.PasswordInput);

      await RespondAsync("Login successful!", ephemeral: true);
   }
}
