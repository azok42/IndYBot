namespace IndYBot.Data.Models;

public partial class AutoEntryHistory
{
    public ulong UserId { get; set; }

    public DateTime ExecutedAt { get; set; }

    public string Status { get; set; } = null!;

    public sbyte EntriesCreated { get; set; }

    public virtual User User { get; set; } = null!;
}
