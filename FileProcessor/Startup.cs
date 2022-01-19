using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProcessor
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using BusinessLogic.Common;
    using BusinessLogic.EventHandling;
    using BusinessLogic.FileFormatHandlers;
    using BusinessLogic.Managers;
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
    using Common;
    using EstateManagement.Client;
    using EstateReporting.Database;
    using EventStore.Client;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using FIleProcessor.Models;
    using HealthChecks.UI.Client;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog.Extensions.Logging;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EntityFramework.ConnectionStringConfiguration;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Extensions;
    using Shared.EventStore.SubscriptionWorker;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Shared.Middleware;
    using Shared.Repositories;
    using Swashbuckle.AspNetCore.Filters;
    using TransactionProcessor.Client;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the web host environment.
        /// </summary>
        /// <value>
        /// The web host environment.
        /// </value>
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        public Startup(IWebHostEnvironment webHostEnvironment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                                      .AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        private static EventStoreClientSettings EventStoreClientSettings;

        private static void ConfigureEventStoreSettings(EventStoreClientSettings settings = null)
        {
            if (settings == null)
            {
                settings = new EventStoreClientSettings();
            }

            settings.CreateHttpMessageHandler = () => new SocketsHttpHandler
                                                      {
                                                          SslOptions =
                                                          {
                                                              RemoteCertificateValidationCallback = (sender,
                                                                                                     certificate,
                                                                                                     chain,
                                                                                                     errors) => true,
                                                          }
                                                      };

            settings.ConnectivitySettings = new EventStoreClientConnectivitySettings
                                            {
                                                Insecure = Startup.Configuration.GetValue<Boolean>("EventStoreSettings:Insecure"),
                                                Address = new Uri(Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString")),
                                            };

            settings.DefaultCredentials = new UserCredentials(Startup.Configuration.GetValue<String>("EventStoreSettings:UserName"),
                                                              Startup.Configuration.GetValue<String>("EventStoreSettings:Password"));
            Startup.EventStoreClientSettings = settings;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            Startup.ConfigureEventStoreSettings();

            this.ConfigureMiddlewareServices(services);

            IEnumerable<FileProfile> fileProfiles = Configuration.GetSection("AppSettings:FileProfiles").GetChildren().ToList().Select(x => new
                {
                    Name = x.GetValue<String>("Name"),
                    FileProfileId = x.GetValue<Guid>("Id"),
                    RequestType = x.GetValue<String>("RequestType"),
                    ListeningDirectory = x.GetValue<String>("ListeningDirectory"),
                    OperatorName = x.GetValue<String>("OperatorName"),
                    LineTerminator = x.GetValue<String>("LineTerminator"),
                    FileFormatHandler = x.GetValue<String>("FileFormatHandler")

            }).Select(f =>
                          {
                              return new FileProfile(f.FileProfileId, f.Name, f.ListeningDirectory, f.RequestType, f.OperatorName, f.LineTerminator, f.FileFormatHandler);
                          });
            services.AddSingleton<List<FileProfile>>(fileProfiles.ToList());


            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IFileProcessorManager, FileProcessorManager>();
            services.AddSingleton<BusinessLogic.Common.IModelFactory, BusinessLogic.Common.ModelFactory>();
            services.AddSingleton<Common.IModelFactory, Common.ModelFactory>();
            services.AddSingleton<IDbContextFactory<EstateReportingGenericContext>, DbContextFactory<EstateReportingGenericContext>>();
            services.AddSingleton<Func<String, EstateReportingGenericContext>>(cont => (connectionString) =>
                                                                                       {
                                                                                           String databaseEngine =
                                                                                               ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

                                                                                           return databaseEngine switch
                                                                                           {
                                                                                               "MySql" => new EstateReportingMySqlContext(connectionString),
                                                                                               "SqlServer" => new EstateReportingSqlServerContext(connectionString),
                                                                                               _ => throw new
                                                                                                   NotSupportedException($"Unsupported Database Engine {databaseEngine}")
                                                                                           };
                                                                                       });

            Boolean useConnectionStringConfig = Boolean.Parse(ConfigurationReader.GetValue("AppSettings", "UseConnectionStringConfig"));

            if (useConnectionStringConfig)
            {
                String connectionStringConfigurationConnString = ConfigurationReader.GetConnectionString("ConnectionStringConfiguration");
                services.AddSingleton<IConnectionStringConfigurationRepository, ConnectionStringConfigurationRepository>();
                services.AddTransient<ConnectionStringConfigurationContext>(c =>
                                                                            {
                                                                                return new ConnectionStringConfigurationContext(connectionStringConfigurationConnString);
                                                                            });

                // TODO: Read this from a the database and set
            }
            else
            {
                services.AddEventStoreClient(Startup.ConfigureEventStoreSettings);
                services.AddEventStoreProjectionManagerClient(Startup.ConfigureEventStoreSettings);
                services.AddEventStorePersistentSubscriptionsClient(Startup.ConfigureEventStoreSettings);
                services.AddSingleton<IConnectionStringConfigurationRepository, ConfigurationReaderConnectionStringRepository>();
            }

            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IEventStoreContext, EventStoreContext>();

            services.AddSingleton<ISecurityServiceClient, SecurityServiceClient>();
            services.AddSingleton<IEstateClient, EstateClient>();
            services.AddSingleton<ITransactionProcessorClient, TransactionProcessorClient>();

            services.AddSingleton<Func<String, String>>(container => (serviceName) =>
                                                                     {
                                                                         return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString;
                                                                     });

            var httpMessageHandler = new SocketsHttpHandler
                                     {
                                         SslOptions =
                                         {
                                             RemoteCertificateValidationCallback = (sender,
                                                                                    certificate,
                                                                                    chain,
                                                                                    errors) => true,
                                         }
                                     };
            HttpClient httpClient = new HttpClient(httpMessageHandler);
            services.AddSingleton(httpClient);

            services.AddSingleton<IAggregateRepository<FileAggregate.FileAggregate, DomainEventRecord.DomainEvent>, AggregateRepository<FileAggregate.FileAggregate, DomainEventRecord.DomainEvent>>();
            services.AddSingleton<IAggregateRepository<FileImportLogAggregate.FileImportLogAggregate, DomainEventRecord.DomainEvent>, AggregateRepository<FileImportLogAggregate.FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            FileCreatedEvent f = new FileCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), String.Empty,
                                                      new DateTime());
            FileAddedToImportLogEvent f1 = new FileAddedToImportLogEvent(Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         String.Empty,
                                                                         String.Empty,
                                                                         new DateTime());

            TypeProvider.LoadDomainEventsTypeDynamically();

            // request & notification handlers
            services.AddTransient<ServiceFactory>(context =>
                                                  {
                                                      return t => context.GetService(t);
                                                  });

            services.AddSingleton<IRequestHandler<UploadFileRequest, Guid>, FileRequestHandler>();
            services.AddSingleton<IRequestHandler<ProcessUploadedFileRequest, Unit>, FileRequestHandler>();
            services.AddSingleton<IRequestHandler<SafaricomTopupRequest, Unit>, FileRequestHandler>();
            services.AddSingleton<IRequestHandler<VoucherRequest, Unit>, FileRequestHandler>();
            services.AddSingleton<IRequestHandler<ProcessTransactionForFileLineRequest, Unit>, FileRequestHandler>();

            Dictionary<String, String[]> eventHandlersConfiguration = new Dictionary<String, String[]>();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration").Bind(eventHandlersConfiguration);
                }
            }
            services.AddSingleton<Dictionary<String, String[]>>(eventHandlersConfiguration);

            services.AddSingleton<Func<Type, IDomainEventHandler>>(container => (type) =>
                                                                                {
                                                                                    IDomainEventHandler handler = container.GetService(type) as IDomainEventHandler;
                                                                                    return handler;
                                                                                });
            
            services.AddSingleton<FileDomainEventHandler>();
            services.AddSingleton<IDomainEventHandlerResolver, DomainEventHandlerResolver>();

            services.AddSingleton<Func<String, IFileFormatHandler>>(container => (fileFormatHandlerName) =>
                                                                                 {
                                                                                     if (fileFormatHandlerName == "SafaricomFileFormatHandler")
                                                                                        return new SafaricomFileFormatHandler();
                                                                                     if (fileFormatHandlerName == "VoucherFileFormatHandler")
                                                                                         return new VoucherFileFormatHandler();

                                                                                     return null;
                                                                                 });

            Startup.ServiceProvider = services.BuildServiceProvider();

        }

        public static IServiceProvider ServiceProvider { get; set; }

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

        private void ConfigureMiddlewareServices(IServiceCollection services)
        {
            services.AddHealthChecks().AddEventStore(Startup.EventStoreClientSettings,
                                                     userCredentials:Startup.EventStoreClientSettings.DefaultCredentials,
                                                     name:"Eventstore",
                                                     failureStatus:HealthStatus.Unhealthy,
                                                     tags:new string[] {"db", "eventstore"}).AddEstateManagementService().AddTransactionProcessorService()
                    .AddSecurityService(ApiEndpointHttpHandler);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "File Processor API",
                    Version = "1.0",
                    Description = "A REST Api to manage importing and processing bulk transaction files.",
                    Contact = new OpenApiContact
                    {
                        Name = "Stuart Ferguson",
                        Email = "golfhandicapping@btinternet.com"
                    }
                });
                // add a custom operation filter which sets default values
                //c.OperationFilter<SwaggerDefaultValues>();
                //c.ExampleFilters();

                //Locate the XML files being generated by ASP.NET...
                var directory = new DirectoryInfo(AppContext.BaseDirectory);
                var xmlFiles = directory.GetFiles("*.xml");

                //... and tell Swagger to use those XML comments.
                foreach (FileInfo fileInfo in xmlFiles)
                {
                    c.IncludeXmlComments(fileInfo.FullName);
                }

            });

            //services.AddSwaggerExamplesFromAssemblyOf<SwaggerJsonConverter>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                                                     {
                                                         ServerCertificateCustomValidationCallback =
                                                             (message, certificate, chain, sslPolicyErrors) => true
                                                     };
                    options.Authority = ConfigurationReader.GetValue("SecurityConfiguration", "Authority");
                    options.Audience = ConfigurationReader.GetValue("SecurityConfiguration", "ApiName");

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                                                        {
                                                            ValidateAudience = false,
                                                            ValidAudience = ConfigurationReader.GetValue("SecurityConfiguration", "ApiName"),
                                                            ValidIssuer = ConfigurationReader.GetValue("SecurityConfiguration", "Authority"),
                                                        };
                    options.IncludeErrorDetails = true;
                });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Culture = new CultureInfo("en-GB");
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            services.AddMvcCore().AddApplicationPart(assembly).AddControllersAsServices();
        }

        public static void LoadTypes()
        {
            FileAddedToImportLogEvent fileAddedToImportLogEvent =
                new FileAddedToImportLogEvent(Guid.Empty,
                                              Guid.Empty,
                                              Guid.Empty,
                                              Guid.Empty,
                                              Guid.Empty,
                                              Guid.Empty,
                                              String.Empty,
                                              String.Empty,
                                              new DateTime());

            FileLineAddedEvent fileLineAddedEvent = new FileLineAddedEvent(Guid.Empty, Guid.Empty, 0, String.Empty);

            TypeProvider.LoadDomainEventsTypeDynamically();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment())
            {
                nlogConfigFilename = $"nlog.{env.EnvironmentName}.config";
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("FileProcessor");

            Logger.Initialise(logger);

            Action<String> loggerAction = message =>
                                          {
                                              Logger.LogInformation(message);
                                          };
            Startup.Configuration.LogConfiguration(loggerAction);

            ConfigurationReader.Initialise(Startup.Configuration);

            //app.AddRequestLogging();
            //app.AddResponseLogging();
            app.AddExceptionHandler();
            app.UseMiddleware<RequestLoggingMiddlewareTemp>();
            app.UseMiddleware<ResponseLoggingMiddlewareTemp>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health", new HealthCheckOptions()
                                                    {
                                                        Predicate = _ => true,
                                                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                    });
            });

            app.UseSwagger();

            app.UseSwaggerUI();

            app.PreWarm();
        }
    }

    public static class Extensions
    {
        static Action<TraceEventType, String, String> log = (tt, subType, message) => {
            String logMessage = $"{subType} - {message}";
            switch (tt)
            {
                case TraceEventType.Critical:
                    Logger.LogCritical(new Exception(logMessage));
                    break;
                case TraceEventType.Error:
                    Logger.LogError(new Exception(logMessage));
                    break;
                case TraceEventType.Warning:
                    Logger.LogWarning(logMessage);
                    break;
                case TraceEventType.Information:
                    Logger.LogInformation(logMessage);
                    break;
                case TraceEventType.Verbose:
                    Logger.LogDebug(logMessage);
                    break;
            }
        };

        static Action<TraceEventType, String> concurrentLog = (tt, message) => log(tt, "CONCURRENT", message);

        public static void PreWarm(this IApplicationBuilder applicationBuilder)
        {
            Startup.LoadTypes();

            //SubscriptionWorker worker = new SubscriptionWorker()
            var internalSubscriptionService = Boolean.Parse(ConfigurationReader.GetValue("InternalSubscriptionService"));

            if (internalSubscriptionService)
            {
                String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");
                Int32 inflightMessages = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "InflightMessages"));
                Int32 persistentSubscriptionPollingInSeconds = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "PersistentSubscriptionPollingInSeconds"));
                String filter = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceFilter");
                String ignore = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceIgnore");
                String streamName = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionFilterOnStreamName");
                Int32 cacheDuration = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceCacheDuration"));

                ISubscriptionRepository subscriptionRepository = SubscriptionRepository.Create(eventStoreConnectionString, cacheDuration);

                ((SubscriptionRepository)subscriptionRepository).Trace += (sender, s) => Extensions.log(TraceEventType.Information, "REPOSITORY", s);

                // init our SubscriptionRepository
                subscriptionRepository.PreWarm(CancellationToken.None).Wait();

                var eventHandlerResolver = Startup.ServiceProvider.GetService<IDomainEventHandlerResolver>();

                SubscriptionWorker concurrentSubscriptions = SubscriptionWorker.CreateConcurrentSubscriptionWorker(eventStoreConnectionString, eventHandlerResolver, subscriptionRepository, inflightMessages, persistentSubscriptionPollingInSeconds);

                concurrentSubscriptions.Trace += (_, args) => concurrentLog(TraceEventType.Information, args.Message);
                concurrentSubscriptions.Warning += (_, args) => concurrentLog(TraceEventType.Warning, args.Message);
                concurrentSubscriptions.Error += (_, args) => concurrentLog(TraceEventType.Error, args.Message);

                if (!String.IsNullOrEmpty(ignore))
                {
                    concurrentSubscriptions = concurrentSubscriptions.IgnoreSubscriptions(ignore);
                }

                if (!String.IsNullOrEmpty(filter))
                {
                    //NOTE: Not overly happy with this design, but
                    //the idea is if we supply a filter, this overrides ignore
                    concurrentSubscriptions = concurrentSubscriptions.FilterSubscriptions(filter)
                                                                     .IgnoreSubscriptions(null);

                }

                if (!String.IsNullOrEmpty(streamName))
                {
                    concurrentSubscriptions = concurrentSubscriptions.FilterByStreamName(streamName);
                }

                concurrentSubscriptions.StartAsync(CancellationToken.None).Wait();
            }
        }
    }
}
