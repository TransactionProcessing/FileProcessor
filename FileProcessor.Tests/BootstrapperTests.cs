using FileProcessor.Testing;

namespace FileProcessor.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Lamar;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using Xunit;

    public class BootstrapperTests
    {
        [Fact(Skip = "re-enable")]
        public void VerifyBootstrapperIsValid()
        {
            Mock<IWebHostEnvironment> hostingEnvironment = new Mock<IWebHostEnvironment>();
            hostingEnvironment.Setup(he => he.EnvironmentName).Returns("Development");
            hostingEnvironment.Setup(he => he.ContentRootPath).Returns("/home");
            hostingEnvironment.Setup(he => he.ApplicationName).Returns("Test Application");

            ServiceRegistry services = new ServiceRegistry();
            Startup s = new Startup(hostingEnvironment.Object);
            Startup.Configuration = this.SetupMemoryConfiguration();

            this.AddTestRegistrations(services, hostingEnvironment.Object);
            s.ConfigureContainer(services);
            Startup.Container.AssertConfigurationIsValid(AssertMode.Full);
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
            //Dictionary<String, String> configuration = new Dictionary<String, String>();

            IConfigurationBuilder builder = new ConfigurationBuilder();

            //configuration.Add("ConnectionStrings:HealthCheck", "HeathCheckConnString");
            //configuration.Add("SecurityConfiguration:Authority", "https://127.0.0.1");
            //configuration.Add("EventStoreSettings:ConnectionString", "https://127.0.0.1:2113");
            //configuration.Add("EventStoreSettings:ConnectionName", "UnitTestConnection");
            //configuration.Add("EventStoreSettings:UserName", "admin");
            //configuration.Add("EventStoreSettings:Password", "changeit");
            //configuration.Add("AppSettings:UseConnectionStringConfig", "false");
            //configuration.Add("AppSettings:SecurityService", "http://127.0.0.1");
            //configuration.Add("AppSettings:MessagingServiceApi", "http://127.0.0.1");
            //configuration.Add("AppSettings:EstateManagementApi", "http://127.0.0.1");
            //configuration.Add("AppSettings:TransactionProcessorApi", "http://127.0.0.1");
            //configuration.Add("AppSettings:DatabaseEngine", "SqlServer");

            //builder.AddInMemoryCollection(configuration);
            builder.AddInMemoryCollection(TestData.DefaultAppSettings);
            return builder.Build();
        }

        private void AddTestRegistrations(ServiceRegistry services,
                                                        IWebHostEnvironment hostingEnvironment)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new DiagnosticListener(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
        }
    }
}