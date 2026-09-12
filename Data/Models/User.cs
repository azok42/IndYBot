namespace IndYBot.Data.Models;

public partial class User
{
    public ulong Id { get; set; }

    public ulong DiscordId { get; set; }

    public bool WhereisEnabled { get; set; }

    public string WhereisVisibility { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AutoEntry> AutoEntries { get; set; } = new List<AutoEntry>();

    public virtual AutoEntryConfig? AutoEntryConfig { get; set; }

    public virtual ICollection<AutoEntryHistory> AutoEntryHistories { get; set; } = new List<AutoEntryHistory>();

    public virtual Credential? Credential { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<GroupEntry> GroupEntries { get; set; } = new List<GroupEntry>();

    public virtual ICollection<GroupEntryMember> GroupEntryMembers { get; set; } = new List<GroupEntryMember>();

    public virtual ICollection<GroupEntryTemplate> GroupEntryTemplates { get; set; } = new List<GroupEntryTemplate>();

    public virtual GroupInvite? GroupInviteInvitee { get; set; }

    public virtual GroupInvite? GroupInviteInviter { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Guild> Guilds { get; set; } = new List<Guild>();
}
