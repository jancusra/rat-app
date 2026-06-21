using Rat.Domain.Infrastructure;

namespace Rat.DataStorage
{
    /// <summary>
    /// Represents database configuration manager
    /// </summary>
    public partial class DatabaseSettingsManager
    {
        private static string _settingsJsonFilePath = @"\App_Data\databaseSettings.json";

        /// <summary>
        /// Get database setting by file path
        /// </summary>
        /// <returns>database settings</returns>
        public static DatabaseSettings GetSettings()
        {
            return CommonSettingsManager.GetSettings<DatabaseSettings>(_settingsJsonFilePath);
        }
    }
}
