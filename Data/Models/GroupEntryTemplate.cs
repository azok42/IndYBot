namespace IndYBot.Data.Models;

public partial class GroupEntryTemplate
{
    public ulong Id { get; set; }

    public ulong GroupId { get; set; }

    public ulong CreatorId { get; set; }

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Teacher { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool Hour3 { get; set; }

    public bool Hour4 { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Creator { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
