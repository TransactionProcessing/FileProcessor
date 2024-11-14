namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using EstateManagement.Client;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using SecurityService.Client;
using Shared.General;
using TransactionProcessor.Client;

[ExcludeFromCodeCoverage]
public class ClientRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientRegistry"/> class.
    /// </summary>
    public ClientRegistry() {
        this.AddSingleton<ISecurityServiceClient, SecurityServiceClient>();
        //this.AddSingleton<IEstateClient, EstateClient>();

        this.AddSingleton<ITransactionProcessorClient, TransactionProcessorClient>();

        //this.AddSingleton<Func<String, String>>(container => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; });
        Func<String, String> resolver(IServiceProvider container) => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; };
        Func<String, String> resolver1() => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; };

        this.AddSingleton<Func<String, String>>(resolver);
        
        SocketsHttpHandler httpMessageHandler = new SocketsHttpHandler {
                                                                           SslOptions = {
                                                                                            RemoteCertificateValidationCallback = (sender,
                                                                                                certificate,
                                                                                                chain,
                                                                                                errors) => true,
                                                                                        }
                                                                       };

        HttpClient httpClient = new(httpMessageHandler);
        this.AddSingleton(httpClient);

        this.AddSingleton<IEstateClient>(new EstateClient(resolver1(), httpClient, 2));
    }

    #endregion
}