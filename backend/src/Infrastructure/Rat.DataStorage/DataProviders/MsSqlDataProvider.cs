using System;
using System.Data.Common;
using System.Data.SqlClient;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;

namespace Rat.DataStorage.DataProviders
{
    /// <summary>
    /// MsSQL data provider definition
    /// </summary>
    public partial class MsSqlDataProvider : BaseDataProvider, IDbDataProvider
    {
        protected override IDataProvider LinqToDbDataProvider => SqlServerTools.GetDataProvider(SqlServerVersion.v2012, SqlServerProvider.MicrosoftDataSqlClient);

        protected override DbConnection GetInternalDbConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException(nameof(connectionString));

            return new SqlConnection(connectionString);
        }
    }
}
