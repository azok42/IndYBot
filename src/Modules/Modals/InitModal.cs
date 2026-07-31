using Discord;
using Discord.Interactions;

namespace IndYBot.Modules.Modals;

public class InitModal : IModal
{
    public string Title { get; } = "Channel setup";

    [InputLabel("Default Channel", "Fallback if you don't set other the channels!")]
    [ModalChannelSelect("default-channel", maxValues: 1)]
    public ITextChannel[]? DefaultChannel { get; set; }

    [InputLabel("Log Channel", "The channel where the logs are being written to!")]
    [ModalChannelSelect("log-channel", maxValues: 1)]
    public ITextChannel[]? LogChannel { get; set; }

    [InputLabel("Auto-Entry Channel", "The channel where users will be notified about their automatic entries!")]
    [ModalChannelSelect("auto_entry-channel", maxValues: 1)]
    public ITextChannel[]? AutoEntryChannel { get; set; }

    [InputLabel("Group-Entry Channel", "The place where all group-entries are. If not set the entry is written to where the command is from!")]
    [ModalChannelSelect("group_entry-channel", maxValues: 1)]
    public ITextChannel[]? GroupEntryChannel { get; set; }
}
