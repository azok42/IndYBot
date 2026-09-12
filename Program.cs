using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using IndYLib.Extensions;

using IndYBot.Bot;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IndYBot;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        ConfigureConfig(builder);
        ConfigureServices(builder.Services, builder.Configuration);

        var host = builder.Build();

        await host.RunAsync();
    }

    private static void ConfigureConfig(HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static void ConfigureServices(
            IServiceCollection services,
            IConfiguration config)
    {
        services.AddSingleton(config);

        services.AddIndyAuth();

        // Discord
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<InteractionHandler>();

        services.AddSingleton(services =>
                {
                var client = services
                .GetRequiredService<DiscordSocketClient>();

                return new InteractionService(client);
                });

        services.AddSingleton(new DiscordSocketConfig
                {
                GatewayIntents = GatewayIntents.AllUnprivileged
                });

        services.AddHostedService<IndyBot>();

        // Database

        // Services

        // ...
    }
}
