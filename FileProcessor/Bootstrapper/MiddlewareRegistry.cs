using HealthMonitoring.Client;
using KurrentDB.Client;
using Microsoft.OpenApi;
using Microsoft.Extensions.Configuration;
using TransactionProcessing.ApiDefaults;

namespace FileProcessor.Bootstrapper
{
    using FileProcessor.Common;
    using Lamar;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using OpenIddict.Validation.AspNetCore;
    using Shared.Authorisation;
    using Shared.EventStore.Extensions;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Serialisation;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.Json;

    [ExcludeFromCodeCoverage]
    public class MiddlewareRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareRegistry"/> class.
        /// </summary>
        public MiddlewareRegistry(IConfiguration configuration)
        {
            String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

            KurrentDBClientSettings eventStoreClientSettings = KurrentDBClientSettings.Create(eventStoreConnectionString);

            this.AddHealthChecks().AddEventStore(eventStoreClientSettings,
                                                 userCredentials: eventStoreClientSettings.DefaultCredentials,
                                                 name:"Eventstore",
                                                 failureStatus:HealthStatus.Unhealthy,
                                                 tags:new[] {"db", "eventstore"}).AddTransactionProcessorService()
                .AddSecurityService(this.ApiEndpointHttpHandler);
            
            this.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            this.AddOpenIddict()
                .AddValidation(options =>
                {
                    // Same as your Authority
                    options.SetIssuer(new Uri(ConfigurationReader.GetValue("SecurityConfiguration", "Authority")));

                    // Enables discovery and HTTP backchannel support
                    options.UseSystemNetHttp()
                        .ConfigureHttpClientHandler(handler =>
                        {
                            // DEV ONLY: bypass all certificate errors
                            handler.ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        });

                    // Register the ASP.NET Core integration
                    options.UseAspNetCore();

                    // Optionally set expected audience(s):
                    options.AddAudiences(ConfigurationReader.GetValue("SecurityConfiguration", "ApiName"));

                });

            this.AddAuthorization();

            this.AddControllers();
            this.ConfigureHttpJsonOptions(options => {
                JsonSerializerConfiguration.ConfigureMinimalApi(options.SerializerOptions);
            });

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            this.AddMvcCore().AddApplicationPart(assembly).AddControllersAsServices();

            this.AddClientCredentialsOnlyPolicy();
            this.AddClientCredentialsHandler();
            
            this.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = 16 * 1024;
                options.MultipartBodyLengthLimit = UploadFileSettings.GetMaximumMultipartRequestSizeBytes();
                options.MemoryBufferThreshold = 64 * 1024;
            });

            this.AddHealthMonitoringRegistration(configuration);

            this.AddApiDefaults(options => {
                options.Title = "File Processor";
                options.Description = "API for all aspects of bulk file processing";
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// APIs the endpoint HTTP handler.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        private HttpClientHandler ApiEndpointHttpHandler(IServiceProvider serviceProvider)
        {
            return new HttpClientHandler
                   {
                       ServerCertificateCustomValidationCallback = (message,
                                                                    cert,
                                                                    chain,
                                                                    errors) =>
                                                                   {
                                                                       return true;
                                                                   }
                   };
        }

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public static class JsonSerializerConfiguration
    {
        public static void ConfigureMinimalApi(JsonSerializerOptions serializerOptions)
        {
            var defaultOptions = SystemTextJsonSerializer.GetDefaultJsonSerializerOptions();
            serializerOptions.PropertyNamingPolicy = defaultOptions.PropertyNamingPolicy;
            serializerOptions.DictionaryKeyPolicy = defaultOptions.DictionaryKeyPolicy;
            serializerOptions.ReferenceHandler = defaultOptions.ReferenceHandler;
            serializerOptions.WriteIndented = defaultOptions.WriteIndented;
            serializerOptions.Converters.Add(new DateTimeSpaceConverter());
        }
    }
}
