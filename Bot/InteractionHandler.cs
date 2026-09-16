using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using IndYBot.Responses;

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
                _logger.LogError("{Command}: Unmet precoondition: {Reason}", command.Name, result.ErrorReason);
                await new ErrorResponse(
                        result.ErrorReason,
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            case InteractionCommandError.UnknownCommand:
                _logger.LogError("{Command}: Unknown command", command.Name);
                await new ErrorResponse(
                        "Unkown command?",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            case InteractionCommandError.BadArgs:
                _logger.LogError("{Command}: Bad args", command.Name);
                await new ErrorResponse(
                        "Invalid arguments given.",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            case InteractionCommandError.ConvertFailed:
                _logger.LogError("{Command}: Convert failed", command.Name);
                await new ErrorResponse(
                        "Invalid format for a parameter.",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            case InteractionCommandError.ParseFailed:
                _logger.LogError("{Command}: Parse failed", command.Name);
                await new ErrorResponse(
                        "Unable to parse command context.",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            case InteractionCommandError.Exception:
                _logger.LogError("{Command}: Exception", command.Name);
                await new ErrorResponse(
                        $"Command had an internal error: {result.ErrorReason}",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;

            default:
                _logger.LogError("{Command}: Error", command.Name);
                await new ErrorResponse(
                        $"Command failed: {result.ErrorReason}",
                        LogLevel.Error)
                    .SendAsync(ctx);
                break;
        }
    }

    private async Task HandleDisconnect(Exception e)
    {
        disconnectMsg = e.Message;
        disconnectTime = DateTime.Now;
    }
}
