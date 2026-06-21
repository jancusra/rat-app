using FluentMigrator.Runner.Initialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rat.DataStorage
{
    /// <summary>
    /// Represents database source configuration
    /// </summary>
    public partial class DatabaseSettings : IConnectionStringAccessor
    {
        /// <summary>
        /// Connection string
        /// </summary>
        [JsonProperty(PropertyName = "DataConnectionString")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Database data provider identifier
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseType DataProvider { get; set; }
    }
}
