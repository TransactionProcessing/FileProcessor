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
    using SecurityService.Client;
    using Xunit;

    public class BootstrapperTests
    {
        [Fact(Skip = "Needs investigation")]
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
            IConfigurationBuilder builder = new ConfigurationBuilder();

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