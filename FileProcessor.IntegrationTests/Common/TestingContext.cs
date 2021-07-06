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
    using TechTalk.SpecFlow;

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
        private readonly List<EstateDetails> Estates;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingContext"/> class.
        /// </summary>
        public TestingContext()
        {
            this.Estates = new List<EstateDetails>();
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

        /// <summary>
        /// Adds the client details.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="grantType">Type of the grant.</param>
        public void AddClientDetails(String clientId,
                                     String clientSecret,
                                     String grantType)
        {
            this.Clients.Add(ClientDetails.Create(clientId, clientSecret, grantType));
        }

        /// <summary>
        /// Adds the estate details.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="estateName">Name of the estate.</param>
        public void AddEstateDetails(Guid estateId,
                                     String estateName)
        {
            this.Estates.Add(EstateDetails.Create(estateId, estateName));
        }

        /// <summary>
        /// Gets all estate ids.
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetAllEstateIds()
        {
            return this.Estates.Select(e => e.EstateId).ToList();
        }

        /// <summary>
        /// Gets the client details.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public ClientDetails GetClientDetails(String clientId)
        {
            ClientDetails clientDetails = this.Clients.SingleOrDefault(c => c.ClientId == clientId);

            clientDetails.ShouldNotBeNull();

            return clientDetails;
        }

        /// <summary>
        /// Gets the estate details.
        /// </summary>
        /// <param name="tableRow">The table row.</param>
        /// <returns></returns>
        public EstateDetails GetEstateDetails(TableRow tableRow)
        {
            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
            EstateDetails estateDetails = null;

            estateDetails = this.Estates.SingleOrDefault(e => e.EstateName == estateName);

            if (estateDetails == null && estateName == "InvalidEstate")
            {
                estateDetails = EstateDetails.Create(Guid.Parse("79902550-64DF-4491-B0C1-4E78943928A3"), estateName);
                estateDetails.AddMerchant(Guid.Parse("36AA0109-E2E3-4049-9575-F507A887BB1F"), "Test Merchant 1");
                this.Estates.Add(estateDetails);
            }

            estateDetails.ShouldNotBeNull();

            return estateDetails;
        }

        /// <summary>
        /// Gets the estate details.
        /// </summary>
        /// <param name="estateName">Name of the estate.</param>
        /// <returns></returns>
        public EstateDetails GetEstateDetails(String estateName)
        {
            EstateDetails estateDetails = this.Estates.SingleOrDefault(e => e.EstateName == estateName);

            estateDetails.ShouldNotBeNull();

            return estateDetails;
        }

        /// <summary>
        /// Gets the estate details.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <returns></returns>
        public EstateDetails GetEstateDetails(Guid estateId)
        {
            EstateDetails estateDetails = this.Estates.SingleOrDefault(e => e.EstateId == estateId);

            estateDetails.ShouldNotBeNull();

            return estateDetails;
        }

        #endregion

        public void GenerateFile(String fileName,
                                 Table table)
        {
            StringBuilder fileBuilder = new StringBuilder();

            Int32 currentRow = 1;
            foreach (var row in table.Rows)
            {
                StringBuilder rowBuilder = new StringBuilder();
                foreach (String rowValue in row.Values)
                {
                    rowBuilder.Append($"{rowValue},");
                }
                // remove the trailing comma
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
                if (currentRow < table.Rows.Count)
                {
                    rowBuilder.Append("\n");
                }

                fileBuilder.Append(rowBuilder.ToString());
                currentRow++;
            }

            Directory.CreateDirectory("/home/runner/specflow");
            Guid fileNameId = Guid.NewGuid();
            String filepath = $"/home/runner/specflow/{fileName}";
            // Should have the whole file here
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.WriteAsync(fileBuilder.ToString());
            }

            this.UploadFile = filepath;
        }
    }
}
