using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebMaestro.Helpers;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Services
{
    internal class FileService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private string WebMaestroFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".webmaestro");

        public async Task SaveRequest(CollectionModel collection, System.Guid id, RequestModel request)
        {
            var path = Path.GetDirectoryName(collection.Path);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Collection path is invalid.");
            }

            // Sanitize the filename by replacing path separators with safe characters
            var sanitizedName = request.Name.Replace('/', '-').Replace('\\', '-');
            var filename = Path.Combine(path, sanitizedName + ".req");

            await FileHelpers.SaveJsonFileAsync(filename, request);

            collection.Files.Add(new()
            {
                Name = request.Name,  // Keep original name with slashes for display
                FileName = filename,   // Use sanitized filename for the actual file path
                Url = request.Url,
                HttpMethod = request.HttpMethod,
                Id = id
            });
        }

        internal async Task SaveOpenTabsAsync(List<TabItemViewModel> tabs)
        {
            if (tabs.Count == 0)
            {
                return;
            }

            var path = WebMaestroFolderPath;

            Directory.CreateDirectory(path);

            foreach (string file in Directory.EnumerateFiles(path, "tmp_*.json"))
            {
                File.Delete(file);
            }

            var docs = new List<DocumentModel>();

            foreach (var tab in tabs.OfType<WebViewModel>())
            {
                var filename = $"tmp_{tab.Id}.json";
                var fullPath = Path.Combine(path, filename);
                await FileHelpers.SaveJsonFileAsync(fullPath, tab.Request);

                docs.Add(new DocumentModel
                {
                    Id = tab.Id,
                    Type = tab.GetType().Name,
                    File = filename,
                });
            }

            await FileHelpers.SaveJsonFileAsync(Path.Combine(path, "openTabs.json"), docs);
        }

        internal async Task<List<DocumentModel>> LoadOpenTabsAsync()
        {
            var path = Path.Combine(WebMaestroFolderPath, "openTabs.json");
            if (!File.Exists(path))
            {
                return new List<DocumentModel>();
            }

            return await FileHelpers.ReadJsonFileAsync<List<DocumentModel>>(path) ?? new List<DocumentModel>();
        }

        internal async Task<T?> ReadTempFileAsync<T>(string filename)
        {
            var path = Path.Combine(WebMaestroFolderPath, filename);
            if (!File.Exists(path))
            {
                return default;
            }

            return await FileHelpers.ReadJsonFileAsync<T>(path);
        }

        internal async Task SaveTempFileAsync<T>(string filename, T obj)
        {
            Directory.CreateDirectory(WebMaestroFolderPath);
            var path = Path.Combine(WebMaestroFolderPath, filename);
            await FileHelpers.SaveJsonFileAsync(path, obj);
        }

        internal async Task<AppStateModel> LoadAppStateAsync()
        {
            var path = Path.Combine(WebMaestroFolderPath, "appstate.json");
            if (!File.Exists(path))
            {
                return new();
            }

            return await FileHelpers.ReadJsonFileAsync<AppStateModel>(path) ?? new();
        }

        internal async Task SaveAppStateAsync(AppStateModel appState)
        {
            Directory.CreateDirectory(WebMaestroFolderPath);
            var path = Path.Combine(WebMaestroFolderPath, "appstate.json");
            await FileHelpers.SaveJsonFileAsync(path, appState);
        }
    }
}
