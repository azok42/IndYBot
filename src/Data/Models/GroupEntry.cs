namespace IndYBot.Data.Models;

public partial class GroupEntry
{
   public ulong Id { get; set; }

   public ulong GroupId { get; set; }

   public ulong CreatorId { get; set; }

   public DateTime EntryDate { get; set; }

   public string Subject { get; set; } = null!;

   public string Teacher { get; set; } = null!;

   public string Description { get; set; } = null!;

   public bool Hour3 { get; set; }

   public bool Hour4 { get; set; }

   public string Status { get; set; } = null!;

   public DateTime CreatedId { get; set; }

   public DateTime? ClosedAt { get; set; }

   public virtual User Creator { get; set; } = null!;

   public virtual Group Group { get; set; } = null!;

   public virtual ICollection<GroupEntryMember> GroupEntryMembers { get; set; } = new List<GroupEntryMember>();
}
