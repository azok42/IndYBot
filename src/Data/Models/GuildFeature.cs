namespace IndYBot.Data.Models;

public partial class GuildFeature
{
   public ulong GuildId { get; set; }

   public string Feature { get; set; } = null!;

   public bool? Enabled { get; set; }

   public virtual Guild Guild { get; set; } = null!;
}
