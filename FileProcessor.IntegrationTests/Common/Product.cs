using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace FileProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Product
    {
        #region Properties

        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        /// <value>
        /// The display text.
        /// </value>
        public String DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the transaction fees.
        /// </summary>
        /// <value>
        /// The transaction fees.
        /// </value>
        public List<TransactionFee> TransactionFees { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public Decimal? Value { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the transaction fee.
        /// </summary>
        /// <param name="transactionFeeId">The transaction fee identifier.</param>
        /// <param name="calculationType">Type of the calculation.</param>
        /// <param name="description">The description.</param>
        /// <param name="value">The value.</param>
        public void AddTransactionFee(Guid transactionFeeId,
                                      CalculationType calculationType,
                                      String description,
                                      Decimal value)
        {
            TransactionFee transactionFee = new TransactionFee
                                            {
                                                TransactionFeeId = transactionFeeId,
                                                CalculationType = calculationType,
                                                Description = description,
                                                Value = value
                                            };

            if (this.TransactionFees == null)
            {
                this.TransactionFees = new List<TransactionFee>();
            }

            this.TransactionFees.Add(transactionFee);
        }

        /// <summary>
        /// Gets the transaction fee.
        /// </summary>
        /// <param name="transactionFeeId">The transaction fee identifier.</param>
        /// <returns></returns>
        public TransactionFee GetTransactionFee(Guid transactionFeeId)
        {
            return this.TransactionFees.SingleOrDefault(t => t.TransactionFeeId == transactionFeeId);
        }

        /// <summary>
        /// Gets the transaction fee.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public TransactionFee GetTransactionFee(String description)
        {
            return this.TransactionFees.SingleOrDefault(t => t.Description == description);
        }

        #endregion
    }
}