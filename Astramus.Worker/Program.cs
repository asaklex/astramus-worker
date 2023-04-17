using Astramus.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions();

        var config = hostContext.Configuration;

        services.Configure<MasterConf>(config.GetSection("MasterConf"));

        services.AddHostedService<WindowsBackgroundService>();
        services.AddWindowsService(opts =>
        {
            opts.ServiceName = ".Net Joke Service";
        });

        services.AddSingleton<JokeService>();
    })
    .Build();



host.Run();
