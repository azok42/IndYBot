namespace IndYBot.Data.Models;

public partial class GroupEntryMember
{
    public ulong GroupEntryId { get; set; }

    public ulong UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual GroupEntry GroupEntry { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
