using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WebMaestro.Helpers;
using WebMaestro.Models;
using System.Linq;

namespace WebMaestro.Services
{
    internal class CollectionsService : ObservableObject
    {
        private ObservableCollection<CollectionModel> collections = new();

        public ObservableCollection<CollectionModel> Collections
        {
            get => collections;
            set => SetProperty(ref collections, value);
        }

        public async Task LoadCollectionsAsync()
        {
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebMaestro", "WebMaestro.collections");

            if (!File.Exists(filename))
            {
                return;
            }

            var contexts = await FileHelpers.ReadJsonFileAsync<List<CollectionContext>>(filename);

            contexts.ForEach(x =>
            {
                var collection = FileHelpers.ReadJsonFile<CollectionModel>(x.Filename);
                collection.Path = x.Filename;
                this.Collections.Add(collection);
            });
        }

        private async Task SaveCollectionsAsync()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebMaestro");
            var filename = Path.Combine(path, "WebMaestro.collections");

            _ = Directory.CreateDirectory(path);

            var contexts = new List<CollectionContext>();

            foreach (var collection in this.Collections)
            {
                contexts.Add(new CollectionContext()
                {
                    Name = collection.Name,
                    Filename = collection.Path
                });
            }

            await FileHelpers.SaveJsonFileAsync(filename, contexts);
        }

        public async Task<CollectionModel> CreateCollectionAsync(string collectionName, string location)
        {
            var collection = new CollectionModel()
            {
                Name = collectionName
            };

            return await CreateCollectionAsync(collection, location);
        }

        public async Task<CollectionModel> CreateCollectionAsync(CollectionModel collection, string location)
        {
            var path = Path.Combine(location, collection.Name);

            _ = Directory.CreateDirectory(path);

            var filename = Path.Combine(path, collection.Name + ".coll");

            collection.Path = filename;

            this.Collections.Add(collection);

            await FileHelpers.SaveJsonFileAsync(filename, collection);

            await this.SaveCollectionsAsync();

            return collection;
        }

        public async Task LoadCollectionAsync(string filename)
        {
            var collection = await FileHelpers.ReadJsonFileAsync<CollectionModel>(filename);
            collection.Path = filename;

            this.Collections.Add(collection);

            await this.SaveCollectionsAsync();
        }

        public async Task SaveCollectionAsync(CollectionModel collectionModel)
        {
            await FileHelpers.SaveJsonFileAsync(collectionModel.Path, collectionModel);
        }

        public async Task RemoveCollection(CollectionModel collection)
        {
            this.Collections.Remove(collection);

            await this.SaveCollectionsAsync();
        }

        public async void DeleteRequestAsync(CollectionModel collectionModel, CollectionFileModel collectionFileModel)
        {
            File.Delete(collectionFileModel.FileName);
            collectionModel.Files.Remove(collectionFileModel);

            await this.SaveCollectionAsync(collectionModel);
        }

        public async Task AddRequestAsync(CollectionModel collectionModel)
        {
            var name = "New Request";

            var req = new RequestModel()
            {
                Name = name
            };

            await AddRequestAsync(collectionModel, req);
        }

        public async Task AddRequestAsync(CollectionModel collectionModel, RequestModel req)
        {
            var file = new CollectionFileModel()
            {
                Name = req.Name,
                Url = req.Url,
                HttpMethod = req.HttpMethod
            };

            var path = Path.GetDirectoryName(collectionModel.Path);
            var filename = Path.Combine(path, $"{ req.HttpMethod }-{ req.Name }.req");
            file.FileName = filename;

            await FileHelpers.SaveJsonFileAsync(filename, req);

            collectionModel.Files.Add(file);

            await this.SaveCollectionAsync(collectionModel);
        }

        public async Task AddExistingRequestAsync(CollectionModel collectionModel, string fileName)
        {
            var req = await FileHelpers.ReadJsonFileAsync<RequestModel>(fileName);

            var path = Path.GetDirectoryName(collectionModel.Path);
            var newFilename = Path.Combine(path, Path.GetFileName(fileName));

            var file = new CollectionFileModel()
            {
                Name = req.Name,
                FileName = newFilename,
                Url = req.Url,
                HttpMethod = req.HttpMethod
            };

            await FileHelpers.SaveJsonFileAsync(newFilename, req);

            collectionModel.Files.Add(file);

            await this.SaveCollectionAsync(collectionModel);
        }

    }
}
