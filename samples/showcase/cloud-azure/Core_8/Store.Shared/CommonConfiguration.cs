﻿using System;
using System.Text;
using Microsoft.Azure;
using NServiceBus;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.Features;
using NServiceBus.MessageMutator;

public static class CommonConfiguration
{
    public static void ApplyCommonConfiguration(this EndpointConfiguration endpointConfiguration,
        Action<RoutingSettings<AzureStorageQueueTransport>> messageEndpointMappings = null)
    {
        var connectionString = CloudConfigurationManager.GetSetting("NServiceBus.ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "UseDevelopmentStorage=true";
        }

        var transport = endpointConfiguration.UseTransport(new AzureStorageQueueTransport(connectionString));
        messageEndpointMappings?.Invoke(transport);

        var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();
        persistence.ConnectionString(connectionString);
        var defaultKey = "2015-10";
        var ascii = Encoding.ASCII;
        var encryptionService = new RijndaelEncryptionService(
            encryptionKeyIdentifier: defaultKey,
            key: ascii.GetBytes("gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6"));
        endpointConfiguration.EnableMessagePropertyEncryption(encryptionService);
        endpointConfiguration.AuditProcessedMessagesTo("audit");
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.RegisterMessageMutator(new DebugFlagMutator());
        endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
    }
}