namespace IndYBot.Data.Models;

public partial class Group
{
    public ulong Id { get; set; }

    public ulong GuildId { get; set; }

    public ulong OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public ulong RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<GroupEntry> GroupEntries { get; set; } = new List<GroupEntry>();

    public virtual ICollection<GroupEntryTemplate> GroupEntryTemplates { get; set; } = new List<GroupEntryTemplate>();

    public virtual GroupInvite? GroupInvite { get; set; }

    public virtual Guild Guild { get; set; } = null!;

    public virtual User Owner { get; set; } = null!;
}
