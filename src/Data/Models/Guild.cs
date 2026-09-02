namespace IndYBot.Data.Models;

public partial class Guild
{
   public ulong Id { get; set; }

   public ulong DiscordId { get; set; }

   public string Timezone { get; set; } = null!;

   public DateTime CreatedAt { get; set; }

   public virtual Group? Group { get; set; }

   public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

   public virtual ICollection<GuildFeature> GuildFeatures { get; set; } = new List<GuildFeature>();

   public virtual ICollection<User> Users { get; set; } = new List<User>();
}
