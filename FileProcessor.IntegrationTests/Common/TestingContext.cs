using Shared.IntegrationTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using Shared.Logger;
    using Shouldly;
    using System.IO;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers;
    using TechTalk.SpecFlow;

    public class ClientDetails
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDetails"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="grantType">Type of the grant.</param>
        private ClientDetails(String clientId,
                              String clientSecret,
                              String grantType)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.GrantType = grantType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public String ClientId { get; }

        /// <summary>
        /// Gets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public String ClientSecret { get; }

        /// <summary>
        /// Gets the type of the grant.
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        public String GrantType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="grantType">Type of the grant.</param>
        /// <returns></returns>
        public static ClientDetails Create(String clientId,
                                           String clientSecret,
                                           String grantType)
        {
            return new ClientDetails(clientId, clientSecret, grantType);
        }

        #endregion
    }

    public class TestingContext
    {
        #region Fields

        /// <summary>
        /// The clients
        /// </summary>
        private readonly List<ClientDetails> Clients;

        /// <summary>
        /// The estates
        /// </summary>
        public readonly List<EstateDetails1> Estates;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingContext"/> class.
        /// </summary>
        public TestingContext()
        {
            this.Estates = new List<EstateDetails1>();
            this.Clients = new List<ClientDetails>();
        }

        #endregion

        public String UploadFile { get; set; }

        #region Properties

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public String AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the docker helper.
        /// </summary>
        /// <value>
        /// The docker helper.
        /// </value>
        public DockerHelper DockerHelper { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public NlogLogger Logger { get; set; }

        #endregion

        #region Methods
        public void AddClientDetails(String clientId,
                                     String clientSecret,
                                     String grantType)
        {
            this.Clients.Add( ClientDetails.Create(clientId, clientSecret, grantType));
        }

        public void AddEstateDetails(Guid estateId,
                                     String estateName,
                                     String estateReference){
            EstateDetails estate = EstateDetails.Create(estateId, estateName, estateReference);
            this.Estates.Add(new EstateDetails1(estate));
        }
        
        public ClientDetails GetClientDetails(String clientId)
        {
            ClientDetails clientDetails = this.Clients.SingleOrDefault(c => c.ClientId == clientId);

            clientDetails.ShouldNotBeNull();

            return clientDetails;
        }

        #endregion
    }
}
