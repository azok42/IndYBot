namespace IndYBot.Data.Models;

public partial class GroupInvite
{
    public ulong Id { get; set; }

    public ulong GroupId { get; set; }

    public ulong InviterId { get; set; }

    public ulong InviteeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User Invitee { get; set; } = null!;

    public virtual User Inviter { get; set; } = null!;
}
