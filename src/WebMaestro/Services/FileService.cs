using System.IO;
using System.Threading.Tasks;
using WebMaestro.Helpers;
using WebMaestro.Models;

namespace WebMaestro.Services
{
    internal class FileService
    {
        public async Task SaveRequest(CollectionModel collection, System.Guid id, RequestModel request)
        {
            var path = Path.GetDirectoryName(collection.Path);
            var filename = Path.Combine(path, request.Name + ".req");

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
    }
}
