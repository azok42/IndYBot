namespace IndYBot.Data.Models;

public partial class AutoEntry
{
    public ulong UserId { get; set; }

    public sbyte IndyDay { get; set; }

    public sbyte ExecutionDay { get; set; }

    public string Subject { get; set; } = null!;

    public string Teacher { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
