using FileProcessor.Testing;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests
{
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;

    public class MediatorTests
    {
        private List<IBaseRequest> Requests = new List<IBaseRequest>();

        public MediatorTests()
        {
            this.Requests.Add(TestData.UploadFileCommand);
            this.Requests.Add(TestData.ProcessUploadedFileCommand);
            this.Requests.Add(TestData.ProcessTransactionForFileLineCommand);
            this.Requests.Add(TestData.GetFileQuery);
            this.Requests.Add(TestData.GetImportLogsQuery);
            this.Requests.Add(TestData.GetImportLogQuery);
        }

        [Fact]
        public async Task Mediator_Send_RequestHandled()
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

            List<String> errors = new List<String>();
            IMediator mediator = Startup.Container.GetService<IMediator>();
            foreach (IBaseRequest baseRequest in this.Requests)
            {
                try
                {
                    await mediator.Send(baseRequest);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if (errors.Any() == true)
            {
                String errorMessage = String.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
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
            //configuration.Add("AppSettings:TransactionProcessorApi", "http://127.0.0.1");
            //configuration.Add("AppSettings:DatabaseEngine", "SqlServer");
            //configuration.Add("AppSettings:EmailProxy", "UnitTest");
            //configuration.Add("AppSettings:SMSProxy", "UnitTest");

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
            services.AddSingleton<IConfiguration>(Startup.Configuration);

            services.OverrideServices(s => {
                s.AddSingleton<IFileProcessorDomainService, DummyFileProcessorDomainService>();
                s.AddSingleton<IFileProcessorManager, DummyFileProcessorManager>();
            });
        }
    }

    public record TestAggregate : Aggregate
    {
        public override void PlayEvent(IDomainEvent domainEvent)
        {

        }

        protected override Object GetMetadata()
        {
            return new Object();
        }
    }
}
