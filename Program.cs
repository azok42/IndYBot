using Discord;
using Discord.WebSocket;
using Discord.Interactions;

using IndYLib.Extensions;

using IndYBot.Bot;
using IndYBot.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;

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

        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            var service = type.GetInterface("IBotService");

            if (service == null)
                continue;

            services.AddSingleton(service, type);
        }

        services.AddSingleton<GetService>();

        // ...
    }
}
