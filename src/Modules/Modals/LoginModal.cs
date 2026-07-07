using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public class LoginModal : IModal
{
   public string Title { get; set; } = "Login";

   [ModalTextDisplay]
   public string PasswordInfo {get; set;} = "Your password is generally speaking *not* saved on the server, **BUT** I do **NOT* guarantee safety!";

   [RequiredInput]
   [InputLabel("Username", "Your IndY-Username")]
   [ModalTextInput("username", placeholder: "user.name")]
   public string? UsernameInput { get; set; }

   [RequiredInput]
   [InputLabel("Password", "Your IndY-Password")]
   [ModalTextInput("password", placeholder: "pass1234")]
   public string? PasswordInput { get; set; }
}
