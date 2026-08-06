using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public enum Hour
{
   [ChoiceDisplay("Hour 3")]
   Hour3 = 3,

   [ChoiceDisplay("Hour 4")]
   Hour4 = 4,
   
   [ChoiceDisplay("Both hours")]
   Hour34 = 34
}

public class EntryModal : IModal
{
   public string Title { get; } = "Make a new entry!";

   [RequiredInput]
   [InputLabel("IndY-Day Date", "The date of your entry! Format: 'Month/Day/Year' (Year is optional)")]
   [ModalTextInput("entry-date")]
   public string? Date { get; set; }

   [RequiredInput]
   [InputLabel("Hour", "You entry can either be 1 hour or both!")]
   [ModalSelectMenu("entry-hour", minValues: 1, maxValues: 1)]
   public Hour[]? Hour { get; set; }

   [RequiredInput]
   [InputLabel("Teacher", "The shortname of the teacher!")]
   [ModalTextInput("entry-teacher")]
   public string? TeacherId { get; set; }

   [RequiredInput]
   [InputLabel("Subject", "The shortname of the subject! If unsure, type '/get subjects'")]
   [ModalTextInput("entry-subject")]
   public string? Subject { get; set; }

   [RequiredInput]
   [InputLabel("Activity", "The description of what you are doing!")]
   [ModalTextInput("entry-activity")]
   public string? Activity { get; set; }

   public EntryModal() { }

   public EntryModal(
         string date = "",
         Hour hour = Modals.Hour.Hour34,
         string teacherId = "",
         string subject = "",
         string activity = "")
   {
      Date = date;
      Hour = new Hour[1] { hour };
      TeacherId = teacherId;
      Subject = subject;
      Activity = activity;
   }
}
