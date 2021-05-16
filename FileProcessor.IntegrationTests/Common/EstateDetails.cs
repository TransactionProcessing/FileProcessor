namespace FileProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EstateDetails
    {
        #region Fields

        /// <summary>
        /// The contracts
        /// </summary>
        private List<Contract> Contracts;

        /// <summary>
        /// The merchants
        /// </summary>
        private readonly Dictionary<String, Guid> Merchants;

        /// <summary>
        /// The merchant users
        /// </summary>
        private readonly Dictionary<String, Dictionary<String, String>> MerchantUsers;

        /// <summary>
        /// The merchant users tokens
        /// </summary>
        private Dictionary<String, Dictionary<String, String>> MerchantUsersTokens;

        /// <summary>
        /// The operators
        /// </summary>
        private readonly Dictionary<String, Guid> Operators;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EstateDetails"/> class.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="estateName">Name of the estate.</param>
        private EstateDetails(Guid estateId,
                              String estateName)
        {
            this.EstateId = estateId;
            this.EstateName = estateName;
            this.Merchants = new Dictionary<String, Guid>();
            this.Operators = new Dictionary<String, Guid>();
            this.MerchantUsers = new Dictionary<String, Dictionary<String, String>>();
            //this.TransactionResponses = new Dictionary<(Guid merchantId, String transactionNumber), SerialisedMessage>();
            //this.ReconciliationResponses = new Dictionary<Guid, SerialisedMessage>();
            this.Contracts = new List<Contract>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public String AccessToken { get; private set; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; }

        /// <summary>
        /// Gets the name of the estate.
        /// </summary>
        /// <value>
        /// The name of the estate.
        /// </value>
        public String EstateName { get; }

        /// <summary>
        /// Gets the estate password.
        /// </summary>
        /// <value>
        /// The estate password.
        /// </value>
        public String EstatePassword { get; private set; }

        /// <summary>
        /// Gets the estate user.
        /// </summary>
        /// <value>
        /// The estate user.
        /// </value>
        public String EstateUser { get; private set; }

        /// <summary>
        /// Gets or sets the transaction responses.
        /// </summary>
        /// <value>
        /// The transaction responses.
        /// </value>
        //private Dictionary<(Guid merchantId, String transactionNumber), SerialisedMessage> TransactionResponses { get; }

        /// <summary>
        /// Gets the reconciliation responses.
        /// </summary>
        /// <value>
        /// The reconciliation responses.
        /// </value>
        //private Dictionary<Guid, SerialisedMessage> ReconciliationResponses { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the contract.
        /// </summary>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="operatorId">The operator identifier.</param>
        public void AddContract(Guid contractId,
                                String contractName,
                                Guid operatorId)
        {
            this.Contracts.Add(new Contract
                               {
                                   ContractId = contractId,
                                   Description = contractName,
                                   OperatorId = operatorId,
                               });
        }

        /// <summary>
        /// Adds the merchant.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="merchantName">Name of the merchant.</param>
        public void AddMerchant(Guid merchantId,
                                String merchantName)
        {
            this.Merchants.Add(merchantName, merchantId);
        }

        /// <summary>
        /// Adds the merchant user.
        /// </summary>
        /// <param name="merchantName">Name of the merchant.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public void AddMerchantUser(String merchantName,
                                    String userName,
                                    String password)
        {
            if (this.MerchantUsers.ContainsKey(merchantName))
            {
                Dictionary<String, String> merchantUsersList = this.MerchantUsers[merchantName];
                if (merchantUsersList.ContainsKey(userName) == false)
                {
                    merchantUsersList.Add(userName, password);
                }
            }
            else
            {
                Dictionary<String, String> merchantUsersList = new Dictionary<String, String>();
                merchantUsersList.Add(userName, password);
                this.MerchantUsers.Add(merchantName, merchantUsersList);
            }
        }

        /// <summary>
        /// Adds the merchant user token.
        /// </summary>
        /// <param name="merchantName">Name of the merchant.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="token">The token.</param>
        public void AddMerchantUserToken(String merchantName,
                                         String userName,
                                         String token)
        {
            if (this.MerchantUsersTokens.ContainsKey(merchantName))
            {
                Dictionary<String, String> merchantUsersList = this.MerchantUsersTokens[merchantName];
                if (merchantUsersList.ContainsKey(userName) == false)
                {
                    merchantUsersList.Add(userName, token);
                }
            }
            else
            {
                Dictionary<String, String> merchantUsersList = new Dictionary<String, String>();
                merchantUsersList.Add(userName, token);
                this.MerchantUsersTokens.Add(merchantName, merchantUsersList);
            }
        }

        /// <summary>
        /// Adds the operator.
        /// </summary>
        /// <param name="operatorId">The operator identifier.</param>
        /// <param name="operatorName">Name of the operator.</param>
        public void AddOperator(Guid operatorId,
                                String operatorName)
        {
            this.Operators.Add(operatorName, operatorId);
        }

        ///// <summary>
        ///// Adds the transaction response.
        ///// </summary>
        ///// <param name="merchantId">The merchant identifier.</param>
        ///// <param name="transactionNumber">The transaction number.</param>
        ///// <param name="transactionResponse">The transaction response.</param>
        //public void AddTransactionResponse(Guid merchantId,
        //                                   String transactionNumber,
        //                                   SerialisedMessage transactionResponse)
        //{
        //    this.TransactionResponses.Add((merchantId, transactionNumber), transactionResponse);
        //}

        //public void AddReconciliationResponse(Guid merchantId,
        //                                   SerialisedMessage reconciliationResponse)
        //{
        //    this.ReconciliationResponses.Add(merchantId, reconciliationResponse);
        //}

        /// <summary>
        /// Creates the specified estate identifier.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="estateName">Name of the estate.</param>
        /// <returns></returns>
        public static EstateDetails Create(Guid estateId,
                                           String estateName)
        {
            return new EstateDetails(estateId, estateName);
        }

        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns></returns>
        public Contract GetContract(String contractName)
        {
            if (this.Contracts.Any() == false)
            {
                return null;
            }
            return this.Contracts.Single(c => c.Description == contractName);
        }

        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <param name="contractId">The contract identifier.</param>
        /// <returns></returns>
        public Contract GetContract(Guid contractId)
        {
            return this.Contracts.Single(c => c.ContractId == contractId);
        }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <param name="merchantName">Name of the merchant.</param>
        /// <returns></returns>
        public Guid GetMerchantId(String merchantName)
        {
            if (merchantName == "InvalidMerchant")
            {
                return Guid.Parse("D59320FA-4C3E-4900-A999-483F6A10C69A");
            }

            return this.Merchants.Single(m => m.Key == merchantName).Value;
        }

        /// <summary>
        /// Gets the operator identifier.
        /// </summary>
        /// <param name="operatorName">Name of the operator.</param>
        /// <returns></returns>
        public Guid GetOperatorId(String operatorName)
        {
            return this.Operators.Single(o => o.Key == operatorName).Value;
        }

        ///// <summary>
        ///// Gets the transaction response.
        ///// </summary>
        ///// <param name="merchantId">The merchant identifier.</param>
        ///// <param name="transactionNumber">The transaction number.</param>
        ///// <returns></returns>
        //public SerialisedMessage GetTransactionResponse(Guid merchantId,
        //                                                String transactionNumber)
        //{
        //    KeyValuePair<(Guid merchantId, String transactionNumber), SerialisedMessage> transactionResponse =
        //        this.TransactionResponses.Where(t => t.Key.merchantId == merchantId && t.Key.transactionNumber == transactionNumber).SingleOrDefault();

        //    return transactionResponse.Value;
        //}

        //public SerialisedMessage GetReconciliationResponse(Guid merchantId)
        //{
        //    KeyValuePair<Guid, SerialisedMessage> reconciliationResponse =
        //        this.ReconciliationResponses.Where(t => t.Key == merchantId).SingleOrDefault();

        //    return reconciliationResponse.Value;
        //}

        /// <summary>
        /// Sets the estate user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public void SetEstateUser(String userName,
                                  String password)
        {
            this.EstateUser = userName;
            this.EstatePassword = password;
        }

        /// <summary>
        /// Sets the estate user token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        public void SetEstateUserToken(String accessToken)
        {
            this.AccessToken = accessToken;
        }

        #endregion
    }
}