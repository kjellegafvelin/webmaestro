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
        private string WebMaestroFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".webmaestro");

        public async Task SaveRequest(CollectionModel collection, System.Guid id, RequestModel request)
        {
            var path = Path.GetDirectoryName(collection.Path);
            var filename = Path.Combine(path!, request.Name + ".req");

            await FileHelpers.SaveJsonFileAsync(filename, request);

            collection.Files.Add(new()
            {
                Name = request.Name,
                FileName = filename,
                Url = request.Url,
                HttpMethod = request.HttpMethod,
                Id = id
            });
        }

        internal void SaveOpenTabs(List<TabItemViewModel> tabs)
        {
            if (tabs.Count == 0)
            {
                return;
            }

            var path = WebMaestroFolderPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Delete all temp files
            foreach (string file in Directory.GetFiles(path, "tmp_*.json"))
            {
                File.Delete(file);
            }

            var docs = tabs.Where(x => x is WebViewModel).Select(x =>
            {
                var filename = $"tmp_{x.Id}.json";
                File.WriteAllText(Path.Combine(path, filename), JsonSerializer.Serialize(((WebViewModel)x).Request));

                return new DocumentModel()
                {
                    Id = x.Id,
                    Type = x.GetType().Name,
                    File = filename,
                };
            }).ToList();

            var json = JsonSerializer.Serialize(docs);

            File.WriteAllText(Path.Combine(path, "openTabs.json"), json);
        }

        internal List<DocumentModel> LoadOpenTabs()
        {
            var path = Path.Combine(WebMaestroFolderPath, "openTabs.json");
            if (!File.Exists(path))
            {
                return new List<DocumentModel>();
            }
            var json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                return new List<DocumentModel>();
            }

            return JsonSerializer.Deserialize<List<DocumentModel>>(json) ?? new List<DocumentModel>();
        }

        internal T? ReadTempFile<T>(string filename)
        {
            var path = Path.Combine(WebMaestroFolderPath, filename);
            if (!File.Exists(path))
            {
                return default;
            }
            var json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }

        internal async Task SaveTempFile<T>(string filename, T obj)
        {
            var path = Path.Combine(WebMaestroFolderPath, filename);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj));
        }

        internal AppStateModel LoadAppState()
        {
            var path = Path.Combine(WebMaestroFolderPath, "appstate.json");
            if (!File.Exists(path))
            {
                return new();
            }
            var json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                return new();
            }

            return JsonSerializer.Deserialize<AppStateModel>(json)!;
        }

        internal void SaveAppState(AppStateModel appState)
        {
            var path = Path.Combine(WebMaestroFolderPath, "appstate.json");
            File.WriteAllText(path, JsonSerializer.Serialize(appState));
        }
    }
}
