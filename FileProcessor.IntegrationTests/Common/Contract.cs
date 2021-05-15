namespace FileProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Contract
    {
        #region Properties

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>
        /// The products.
        /// </value>
        public List<Product> Products { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="displayText">The display text.</param>
        /// <param name="value">The value.</param>
        public void AddProduct(Guid productId,
                               String name,
                               String displayText,
                               Decimal? value = null)
        {
            Product product = new Product
                              {
                                  ProductId = productId,
                                  DisplayText = displayText,
                                  Name = name,
                                  Value = value
                              };

            if (this.Products == null)
            {
                this.Products = new List<Product>();
            }

            this.Products.Add(product);
        }

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public Product GetProduct(Guid productId)
        {
            return this.Products.SingleOrDefault(p => p.ProductId == productId);
        }

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Product GetProduct(String name)
        {
            return this.Products.SingleOrDefault(p => p.Name == name);
        }

        #endregion
    }
}