using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Common
{
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.Data.SqlClient;
    using Shared.General;
    using Shared.Repositories;

    [ExcludeFromCodeCoverage]
    public class ConfigurationReaderConnectionStringRepository : IConnectionStringConfigurationRepository
    {
        #region Methods

        /// <summary>
        /// Creates the connection string.
        /// </summary>
        /// <param name="externalIdentifier">The external identifier.</param>
        /// <param name="connectionStringType">Type of the connection string.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task CreateConnectionString(String externalIdentifier,
                                                 ConnectionStringType connectionStringType,
                                                 String connectionString,
                                                 CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This is only required to complete the interface");
        }

        /// <summary>
        /// Deletes the connection string configuration.
        /// </summary>
        /// <param name="externalIdentifier">The external identifier.</param>
        /// <param name="connectionStringType">Type of the connection string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DeleteConnectionStringConfiguration(String externalIdentifier,
                                                              ConnectionStringType connectionStringType,
                                                              CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This is only required to complete the interface");
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="externalIdentifier">The external identifier.</param>
        /// <param name="connectionStringType">Type of the connection string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<String> GetConnectionString(String externalIdentifier,
                                                      ConnectionStringType connectionStringType,
                                                      CancellationToken cancellationToken)
        {
            String connectionString = string.Empty;
            String databaseName = string.Empty;
            switch (connectionStringType)
            {
                case ConnectionStringType.ReadModel:
                    databaseName = "EstateReportingReadModel" + externalIdentifier;
                    connectionString = ConfigurationReader.GetConnectionString("EstateReportingReadModel");
                    break;
                default:
                    throw new NotSupportedException($"Connection String type [{connectionStringType}] is not supported");
            }

            DbConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = databaseName
            };

            return builder.ToString();
        }

        #endregion
    }
}
