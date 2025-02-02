﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.StartupShutdown";
        LogManager.Use<DefaultFactory>().Level(LogLevel.Error);
        #region Program
        Logger.WriteLine("Starting configuration");
        var endpointConfiguration = new EndpointConfiguration("Samples.StartupShutdown");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.EnableFeature<MyFeature>();
        endpointConfiguration.UseTransport(new LearningTransport());

        Logger.WriteLine("Calling Endpoint.Start");
        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        // simulate some activity
        await Task.Delay(500)
            .ConfigureAwait(false);

        Logger.WriteLine("Endpoint is processing messages");
        Logger.WriteLine("Calling IEndpointInstance.Stop");
        await endpointInstance.Stop()
            .ConfigureAwait(false);
        Logger.WriteLine("Finished");
        #endregion
        Console.WriteLine($"Logged information to {Logger.OutputFilePath}");
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }
}