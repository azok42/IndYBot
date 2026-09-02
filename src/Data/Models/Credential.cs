namespace IndYBot.Data.Models;

public partial class Credential
{
   public ulong UserId { get; set; }

   public string Username { get; set; } = null!;

   public string Password { get; set; } = null!;

   public DateTime CreatedAt { get; set; }

   public virtual User User { get; set; } = null!;
}
