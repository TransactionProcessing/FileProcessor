namespace FileProcessor.IntegrationTests.Common
{
    using System;

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
}