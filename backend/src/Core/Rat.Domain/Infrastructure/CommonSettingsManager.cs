using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Rat.Domain.Infrastructure
{
    /// <summary>
    /// Class to define source of web application settings
    /// </summary>
    public partial class CommonSettingsManager
    {
        private static IWebHostEnvironment _webHostEnvironment;

        public static void InitWebHostEnvironment(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Get setting by specific JSON file path
        /// </summary>
        /// <typeparam name="T">type of the setting model</typeparam>
        /// <param name="settingsJsonFilePath">JSON file server location</param>
        /// <returns>settings model as singleton</returns>
        public static T GetSettings<T>(string settingsJsonFilePath) where T : new()
        {
            if (Singleton<T>.Instance != null)
            {
                return Singleton<T>.Instance;
            }

            var resultPath = _webHostEnvironment.ContentRootPath + settingsJsonFilePath;

            if (!File.Exists(resultPath))
            {
                return new T();
            }

            // TODO: later to custom file provider
            using var fileStream = new FileStream(resultPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            var text = streamReader.ReadToEnd();

            var settingsModel = JsonConvert.DeserializeObject<T>(text);

            Singleton<T>.Instance = settingsModel;
            return Singleton<T>.Instance;
        }
    }
}
