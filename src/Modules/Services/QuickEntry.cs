namespace IndYBot.Modules.Services;

public class QuickEntry
{
   public required GroupEntryModule.EntryType Type { get; set;}
   public required string Date { get; set; }
   public required string TeacherId { get; set; }
   public required string Subject { get; set; }
   public required string Description { get; set; }
   public GetterModule.Hour? Hour { get; set; }
}
