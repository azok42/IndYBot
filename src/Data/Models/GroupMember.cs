namespace IndYBot.Data.Models;

public partial class GroupMember
{
   public ulong GroupId { get; set; }

   public ulong UserId { get; set; }

   public string Role { get; set; } = null!;

   public DateTime JoinedAt { get; set; }

   public virtual Guild Group { get; set; } = null!;

   public virtual User User { get; set; } = null!;
}
