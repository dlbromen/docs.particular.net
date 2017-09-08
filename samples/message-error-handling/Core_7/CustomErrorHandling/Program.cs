﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

static class Program
{
    static void Main()
    {
        //required to prevent possible occurrence of .NET Core issue https://github.com/dotnet/coreclr/issues/12668
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.CustomErrorHandling";
        LogManager.Use<DefaultFactory>()
            .Level(LogLevel.Warn);

        var endpointConfiguration = new EndpointConfiguration("Samples.CustomErrorHandling");
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport<LearningTransport>();

        var recoverability = endpointConfiguration.Recoverability();
        recoverability.Delayed(
            customizations: delayedRetriesSettings =>
            {
                delayedRetriesSettings.NumberOfRetries(0);
            });

        #region Registering-Behavior

        var pipeline = endpointConfiguration.Pipeline;
        pipeline.Register(
            behavior: new CustomErrorHandlingBehavior(),
            description: "Manages thrown exceptions instead of delayed retries.");

        #endregion

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press enter to send a message that will throw an exception or \r\n" +
                          "Press [E] key to send a message failing with the custom exception.");
        Console.WriteLine("Press [ESC] key to exit");

        while (true)
        {
            var input = Console.ReadKey();

            var myMessage = new MyMessage
            {
                Id = Guid.NewGuid(),
                ThrowCustomException = input.Key == ConsoleKey.E
            };

            if (input.Key == ConsoleKey.Escape)
            {
                break;
            }
            await endpointInstance.SendLocal(myMessage)
                .ConfigureAwait(false);
        }
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}