using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IndYBot.Bot;

public class InteractionHandler(
        DiscordSocketClient client,
        InteractionService handler,
        IServiceProvider services,
        IConfiguration config,
        ILogger<IndyBot> logger)
{
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _handler = handler;
    private readonly IServiceProvider _services = services;
    private readonly IConfiguration _config = config;
    private readonly ILogger<IndyBot> _logger = logger;

    private static bool commandsRegistered = false;
    private static string disconnectMsg= "";
    private static DateTime? disconnectTime = null;

    public async Task InitAsync()
    {
        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteractionAsync;
        _client.Disconnected += HandleDisconnect;

        _handler.InteractionExecuted += HandleInteractionExecutedAsync;
    }

    private async Task ReadyAsync()
    {
        if (_config["Debug:Enabled"] == "true")
        {
            _logger.LogInformation("Running in DEBUG mode!");   

            var debugChannelIdString = _config["Debug:Channel"];
            if (string.IsNullOrEmpty(debugChannelIdString))
                throw new Exception("Debug channel id was not found!");

            ulong debugChannelId = ulong.Parse(debugChannelIdString);
            if (_client.GetChannel(debugChannelId) is IMessageChannel channel)
                await channel.SendMessageAsync("online in debug mode!");
        }

        if (!string.IsNullOrEmpty(disconnectMsg))
        {
            _logger.LogCritical("Disconnected...");
            disconnectMsg = "";
        }

        if (commandsRegistered)
            return;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        if (_config["Debug:Enabled"] == "true")
        {
            var debugGuildIdString = _config["Debug:Guild"];
            if (string.IsNullOrEmpty(debugGuildIdString))
                throw new Exception("Debug guild id was not found!");

            ulong debugGuildId = ulong.Parse(debugGuildIdString);
            var commands = await _handler.RegisterCommandsToGuildAsync(debugGuildId);

            if (!_logger.IsEnabled(LogLevel.Information))
                return;

            _logger.LogInformation("{Count} commands have been registered", commands.Count);
        }
        else
        {
            await _handler.RegisterCommandsGloballyAsync();
        }

        commandsRegistered = true;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _handler.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while processing interaction: {e}");

            if (interaction.Type == InteractionType.ApplicationCommand && !interaction.HasResponded)
            {
                await interaction.RespondAsync($"Error while processing interaction: {e.GetBaseException()}");
            }
        }
    }

    private async Task HandleInteractionExecutedAsync(ICommandInfo command, IInteractionContext ctx, IResult result)
    {
        if (result.IsSuccess)
            return;

        switch (result.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                await ctx.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
                break;

            case InteractionCommandError.UnknownCommand:
                await ctx.Interaction.RespondAsync("Unkown command?", ephemeral: true);
                break;

            case InteractionCommandError.BadArgs:
                await ctx.Interaction.RespondAsync($"Invalid arguments given.", ephemeral: true);
                break;

            case InteractionCommandError.ConvertFailed:
                await ctx.Interaction.RespondAsync($"Invalid format for a parameter.", ephemeral: true);
                break;

            case InteractionCommandError.ParseFailed:
                await ctx.Interaction.RespondAsync($"Unable to parse command context.", ephemeral: true);
                break;

            case InteractionCommandError.Exception:
                await ctx.Interaction.RespondAsync($"Command had an internal error: {result.ErrorReason}", ephemeral: true);
                break;

            default:
                await ctx.Interaction.RespondAsync($"Command failed: {result.ErrorReason}", ephemeral: true);
                break;
        }
    }

    private async Task HandleDisconnect(Exception e)
    {
        disconnectMsg = e.Message;
        disconnectTime = DateTime.Now;
    }
}
