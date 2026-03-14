using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Infrastructure;
using UGMKBotMAX.Integration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotSettings>(context.Configuration.GetSection(BotSettings.SectionName));
        services.Configure<RoutingSettings>(context.Configuration.GetSection(RoutingSettings.SectionName));
        services.Configure<ReminderSettings>(context.Configuration.GetSection(ReminderSettings.SectionName));
        services.Configure<GoogleSheetsSettings>(context.Configuration.GetSection(GoogleSheetsSettings.SectionName));

        services.AddSingleton<IApplicationClock, ApplicationClock>();
        services.AddSingleton<IRequestRepository, InMemoryRequestRepository>();
        services.AddSingleton<IRequestRouter, StaticRequestRouter>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IGoogleSheetsService, GoogleSheetsService>();
        services.AddSingleton<IMaxBotGateway, MaxBotGateway>();
        services.AddSingleton<RequestOrchestrator>();

        services.AddHostedService<BotWorker>();
        services.AddHostedService<DailyReminderWorker>();
    })
    .Build();

await host.RunAsync();
