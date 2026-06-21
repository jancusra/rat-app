using System;

namespace Rat.DataStorage.DataProviders
{
    /// <summary>
    /// Class for methods managing data providers
    /// </summary>
    public partial class DataProviderManager : IDataProviderManager
    {
        /// <summary>
        /// Get configured data manager
        /// </summary>
        /// <returns>data provider</returns>
        /// <exception cref="Exception"></exception>
        public static IDbDataProvider GetDataProvider()
        {
            var databaseType = DatabaseSettingsManager.GetSettings().DataProvider;

            return databaseType switch
            {
                DatabaseType.SqlServer => new MsSqlDataProvider(),
                DatabaseType.MySql => new MySqlDataProvider(),
                //DataProviderType.PostgreSQL => new PostgreSqlDataProvider(),
                _ => throw new Exception($"Not supported data provider name: '{databaseType}'")
            };
        }

        public IDbDataProvider DataProvider
        {
            get
            {
                return GetDataProvider();
            }
        }
    }
}
