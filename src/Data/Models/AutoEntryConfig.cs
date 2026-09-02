namespace IndYBot.Data.Models;

public partial class AutoEntryConfig
{
   public ulong UserId { get; set; }

   public TimeOnly Time { get; set; }

   public bool Enabled { get; set; }

   public string Notifications { get; set; } = null!;

   public virtual User User { get; set; } = null!;
}
