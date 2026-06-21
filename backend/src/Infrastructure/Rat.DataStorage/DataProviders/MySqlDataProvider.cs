using System;
using System.Data.Common;
using LinqToDB;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using MySql.Data.MySqlClient;

namespace Rat.DataStorage.DataProviders
{
    /// <summary>
    /// MySQL data provider definition
    /// </summary>
    public partial class MySqlDataProvider : BaseDataProvider, IDbDataProvider
    {
        protected override IDataProvider LinqToDbDataProvider => MySqlTools.GetDataProvider(ProviderName.MySqlOfficial);

        protected override DbConnection GetInternalDbConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("A connection string must be supplied.", nameof(connectionString));

            return new MySqlConnection(connectionString);
        }
    }
}
