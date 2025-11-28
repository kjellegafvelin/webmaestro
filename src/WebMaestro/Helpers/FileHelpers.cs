using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebMaestro.Helpers
{
    internal static class FileHelpers
    {
        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public static T? ReadJsonFile<T>(string filename)
        {
            ArgumentNullException.ThrowIfNull(filename);

            var json = File.ReadAllText(filename);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, ReadOptions);
        }

        public static async Task<T?> ReadJsonFileAsync<T>(string filename)
        {
            ArgumentNullException.ThrowIfNull(filename);

            await using var stream = File.OpenRead(filename);
            return await JsonSerializer.DeserializeAsync<T>(stream, ReadOptions).ConfigureAwait(false);
        }

        public static async Task SaveJsonFileAsync<T>(string fileName, T obj)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(obj);

            await using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, obj, WriteOptions);
        }

        internal static async Task<string> OpenFileAsStringAsync(string fileName)
        {
            using var stream = File.Open(fileName, FileMode.Open);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        internal static async Task SaveStringAsFileAsync(string fileName, string body)
        {
            await using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(body).ConfigureAwait(false);
        }
    }
}
