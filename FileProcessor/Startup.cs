using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using FileProcessor.Endpoints;
using ImTools;

namespace FileProcessor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Bootstrapper;
    using Common;
    using HealthChecks.UI.Client;
    using Lamar;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Shared.Middleware;
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
        
        public static Container Container;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureContainer(ServiceRegistry services)
        {
            ConfigurationReader.Initialise(Startup.Configuration);

            TypeProvider.LoadDomainEventsTypeDynamically();

            services.IncludeRegistry<MediatorRegistry>();
            services.IncludeRegistry<DomainEventHandlerRegistry>();
            services.IncludeRegistry<RepositoryRegistry>();
            services.IncludeRegistry<MiddlewareRegistry>();
            services.IncludeRegistry<FileRegistry>();
            services.IncludeRegistry<MiscRegistry>();
            services.IncludeRegistry<ClientRegistry>();
            
            Startup.Container = new Container(services);

            Startup.ServiceProvider = services.BuildServiceProvider();
        }

        public static IServiceProvider ServiceProvider { get; set; }
        
        public static void LoadDomainEventsTypeDynamically(List<string> assemblyFilters = null)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            List<Type> source = new List<Type>();
            foreach (string assemblyFilter in assemblyFilters)
            {
                List<Assembly> filteredAssemblies = assemblies.Where(a => a.FullName.Contains(assemblyFilter) == true).ToList();
                foreach (Assembly a in filteredAssemblies) {
                    assemblies = assemblies.Remove(a);
                }
            }
            source.AddRange(assemblies.SelectMany((Func<Assembly, IEnumerable<Type>>)(a => (IEnumerable<Type>)a.GetTypes())));

            foreach (Type type in source.Where((Func<Type, bool>)(t => t.IsSubclassOf(typeof(DomainEvent)))).OrderBy((Func<Type, string>)(e => e.Name)).ToList())
                TypeMap.AddType(type, type.Name);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ILogger logger = loggerFactory.CreateLogger("FileProcessor");

            Logger.Initialise(logger);

            Startup.Configuration.LogConfiguration(Logger.LogWarning);

            ConfigurationReader.Initialise(Startup.Configuration);
            app.UseMiddleware<TenantMiddleware>();
            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapDomainEventEndpoint();
                endpoints.MapFileEndpoints();
                endpoints.MapFileImportLogEndpoints();

                endpoints.MapHealthChecks("health", new HealthCheckOptions()
                                                    {
                                                        Predicate = _ => true,
                                                        ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                    });
                endpoints.MapHealthChecks("healthui", new HealthCheckOptions()
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
}
