namespace Rat.DataStorage.Migrations
{
    /// <summary>
    /// Resolves the database schema name for the configured data provider
    /// </summary>
    public static class SchemaResolver
    {
        /// <summary>
        /// Schema name used for table/column existence checks.
        /// MySQL has no schemas (provider uses the current database), so it stays null.
        /// </summary>
        public static string SchemaName =>
            DatabaseSettingsManager.GetSettings().DataProvider switch
            {
                DatabaseType.SqlServer => "dbo",
                DatabaseType.PostgreSQL => "public",
                _ => null
            };
    }
}
