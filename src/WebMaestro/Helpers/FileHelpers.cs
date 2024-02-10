using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebMaestro.Helpers
{
    internal static class FileHelpers
    {
        public static T ReadJsonFile<T>(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            var json = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public static async Task<T> ReadJsonFileAsync<T>(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            await using (var stream = File.OpenRead(filename))
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, options).ConfigureAwait(false);
            }
        }

        public static async Task SaveJsonFileAsync<T>(string fileName, T obj)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            await using (var sw = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await JsonSerializer.SerializeAsync<T>(sw, obj,
                    new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
            }
        }

        internal static async Task<string> OpenFileAsStringAsync(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        internal static async Task SaveStringAsFileAsync(string fileName, string body)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            
            using (var stream = File.OpenWrite(fileName))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(body).ConfigureAwait(false);
            }
        }
    }
}
