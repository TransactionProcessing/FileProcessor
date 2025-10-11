using ClientProxyBase;

namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
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
        this.AddHttpContextAccessor();
        this.RegisterHttpClient<ISecurityServiceClient, SecurityServiceClient>();
        this.RegisterHttpClient<ITransactionProcessorClient, TransactionProcessorClient>();

        Func<String, String> resolver(IServiceProvider container) => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; };
        
        this.AddSingleton<Func<String, String>>(resolver);
    }

    #endregion
}