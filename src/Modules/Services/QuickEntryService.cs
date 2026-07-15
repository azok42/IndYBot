using System.Collections.Concurrent;

namespace IndYBot.Modules.Services;

public class QuickEntryService
{
   public ConcurrentDictionary<Guid, QuickEntry> PendingEntries { get; } = new();
}
