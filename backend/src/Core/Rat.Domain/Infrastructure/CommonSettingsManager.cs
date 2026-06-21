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

            // Normalize the relative path so it works on both Windows ("\") and Linux ("/") hosts.
            var relativePath = settingsJsonFilePath
                .TrimStart('\\', '/')
                .Replace('\\', Path.DirectorySeparatorChar);
            var resultPath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePath);

            if (!File.Exists(resultPath))
            {
                return new T();
            }

            // Cache atomically: concurrent callers read the file at most once.
            return Singleton<T>.GetOrCreate(() =>
            {
                // TODO: later to custom file provider
                using var fileStream = new FileStream(resultPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                var text = streamReader.ReadToEnd();

                return JsonConvert.DeserializeObject<T>(text);
            });
        }
    }
}
