using System;

namespace FileProcessor.DataTransferObjects
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// 
    /// </summary>
    public class UploadFileRequest
    {
        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        [JsonProperty("file_profile_id")]
        public Guid FileProfileId { get; set; }

        /// <summary>
        /// Gets or sets the upload date time.
        /// </summary>
        /// <value>
        /// The upload date time.
        /// </value>
        [JsonProperty("upload_date_time")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime UploadDateTime { get; set; }
    }

    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }
}
