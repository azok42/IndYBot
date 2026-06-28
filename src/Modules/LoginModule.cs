using Discord.Interactions;
using IndYBot.Modals;
using IndYLib.Interfaces;

namespace IndYBot.Modules;

public class LoginModule : InteractionModuleBase<SocketInteractionContext>
{
   private readonly IIndyAuth _indyAuth;

   public LoginModule(IIndyAuth indyAuth)
   {
      _indyAuth = indyAuth;
   }

   [SlashCommand("modal", "test modals")]
   public async Task ModalTestCommand()
   {
      await RespondWithModalAsync<LoginModal>("login-modal");
   }

   [ModalInteraction("login-modal")]
   public async Task HandleLoginModal(LoginModal modal)
   {
      if (modal.UsernameInput == null || modal.PasswordInput == null)
         throw new NullReferenceException("Input is null");
      
      var client = await _indyAuth.CreateClientAsync(modal.UsernameInput, modal.PasswordInput);

      await RespondAsync("Login successful!", ephemeral: true);
      await ReplyAsync("email: " + (await client.GetStudentAsync()).First().EMail);
   }
}
