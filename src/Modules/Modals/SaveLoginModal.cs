using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public class SaveLoginModal : IModal
{
   public string Title { get; set; } = "Save login data";

   [ModalTextDisplay]
   public string PasswordInfo { get; set; } = "Your password is going to be saved encrypted using AES-256. But I won't ensure you it's safe!";

   [RequiredInput(true)]
   [InputLabel("Username", "Your IndY-Username")]
   [ModalTextInput("username", placeholder: "user.name")]
   public string? UsernameInput { get; set; }

   [RequiredInput(true)]
   [InputLabel("Password", "Your IndY-Password")]
   [ModalTextInput("password", placeholder: "pass1234")]
   public string? PasswordInput { get; set; }
}
