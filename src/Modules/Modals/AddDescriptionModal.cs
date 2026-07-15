using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public class AddDescriptionModal : IModal
{
   public string Title { get; set; } = "Add a description";

   [ModalTextDisplay]
   public string Info { get; set; } = "The following description will be used to make the entry where you clicked!";

   [RequiredInput(true)]
   [InputLabel("Description")]
   [ModalTextInput("description", placeholder: "Doing math homework")]
   public string? DescriptionInput { get; set; }
}
