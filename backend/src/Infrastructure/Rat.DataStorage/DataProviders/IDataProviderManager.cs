namespace Rat.DataStorage.DataProviders
{
    public partial interface IDataProviderManager
    {
        /// <summary>
        /// Property to get actual data provider
        /// </summary>
        IDbDataProvider DataProvider { get; }
    }
}
