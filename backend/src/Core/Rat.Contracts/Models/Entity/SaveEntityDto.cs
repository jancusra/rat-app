using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rat.Contracts.Converters;

namespace Rat.Contracts.Models.Entity
{
    /// <summary>
    /// Represents a model for saving a common entity
    /// </summary>
    public partial class SaveEntityDto
    {
        public SaveEntityDto()
        {
            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Language ID
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Entity data to save
        /// </summary>
        [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
        public Dictionary<string, object> Data { get; set; }
    }
}
