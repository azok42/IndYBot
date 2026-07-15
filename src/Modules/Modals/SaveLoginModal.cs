using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public class SaveLoginModal : IModal
{
   public string Title { get; set; } = "Save login data";

   [ModalTextDisplay]
   public string SaveInfo { get; set; } = "You don't *need* to save your password if you don't want, but you still need to write it at logins!";

   [ModalTextDisplay]
   public string PasswordInfo { get; set; } = "Your password is going to be saved in **plaintext!!** So don't think it's safe!";

   [RequiredInput(true)]
   [InputLabel("Username", "Your IndY-Username")]
   [ModalTextInput("username", placeholder: "user.name")]
   public string? UsernameInput { get; set; }

   [RequiredInput(false)]
   [InputLabel("Password", "Your IndY-Password")]
   [ModalTextInput("password", placeholder: "pass1234")]
   public string? PasswordInput { get; set; } = null;
}
