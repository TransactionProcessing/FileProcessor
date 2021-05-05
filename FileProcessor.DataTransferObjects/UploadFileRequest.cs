using System;

namespace FileProcessor.DataTransferObjects
{
    using Newtonsoft.Json;

    public class UploadFileRequest
    {
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [JsonProperty("file_profile_id")]
        public Guid FileProfileId { get; set; }
    }
}
