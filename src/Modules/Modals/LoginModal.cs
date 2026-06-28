using Discord.Interactions;

namespace IndYBot.Modals;

public class LoginModal : IModal
{
   public string Title { get; set; } = "Login";

   [RequiredInput]
   [ModalTextInput("username", placeholder: "user.name")]
   public string? UsernameInput { get; set; }

   [RequiredInput]
   [ModalTextInput("password", placeholder: "pass1234")]
   public string? PasswordInput { get; set; }
}
