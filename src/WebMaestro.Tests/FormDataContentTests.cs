using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebMaestro.Helpers;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels;
using Xunit;

namespace WebMaestro.Tests
{
    public class FormDataContentTests
    {
        [Fact]
        public async Task Request_RoundTrip_Should_Persist_FormData_FilePath()
        {
            var request = new RequestModel
            {
                Name = "test",
                Url = "https://api.example.com/upload",
                HttpMethod = HttpMethods.POST,
            };

            request.FormData.Add(new FormDataModel("file", string.Empty)
            {
                FilePath = @"C:\temp\upload.txt"
            });

            var fileName = Path.Combine(Path.GetTempPath(), $"wm-{Guid.NewGuid():N}.req");
            try
            {
                await FileHelpers.SaveJsonFileAsync(fileName, request);
                var roundTripped = await FileHelpers.ReadJsonFileAsync<RequestModel>(fileName);

                Assert.NotNull(roundTripped);
                Assert.Single(roundTripped.FormData);
                Assert.Equal(@"C:\temp\upload.txt", roundTripped.FormData[0].FilePath);
                Assert.True(roundTripped.FormData[0].IsFile);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        [Fact]
        public async Task CreateContent_Multipart_Should_Include_File_Part()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"wm-{Guid.NewGuid():N}.txt");
            await File.WriteAllTextAsync(tempFile, "file-content");

            try
            {
                var request = new RequestModel
                {
                    Url = "https://api.example.com/upload",
                    HttpMethod = HttpMethods.POST,
                    BodyType = RequestBodyType.Form,
                    FormEncoding = FormEncoding.Multipart
                };

                request.FormData.Add(new FormDataModel("description", "sample"));
                request.FormData.Add(new FormDataModel("file", string.Empty)
                {
                    FilePath = tempFile
                });

                var service = new HttpRequestService();
                using var content = service.CreateContent(null, request);

                var multipart = Assert.IsType<MultipartFormDataContent>(content);
                Assert.NotNull(multipart.Headers.ContentType);
                Assert.Equal("multipart/form-data", multipart.Headers.ContentType!.MediaType);
                Assert.Contains(multipart.Headers.ContentType.Parameters, parameter => parameter.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void CreateContent_Multipart_Should_Throw_When_File_Missing()
        {
            var request = new RequestModel
            {
                Url = "https://api.example.com/upload",
                HttpMethod = HttpMethods.POST,
                BodyType = RequestBodyType.Form,
                FormEncoding = FormEncoding.Multipart
            };

            request.FormData.Add(new FormDataModel("file", string.Empty)
            {
                FilePath = @"C:\does-not-exist.txt"
            });

            var service = new HttpRequestService();

            Assert.Throws<InvalidOperationException>(() => service.CreateContent(null, request));
        }

        [Fact]
        public async Task CreateContent_UrlEncoded_Should_Skip_File_Parts()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"wm-{Guid.NewGuid():N}.txt");
            await File.WriteAllTextAsync(tempFile, "file-content");

            try
            {
                var request = new RequestModel
                {
                    Url = "https://api.example.com/form",
                    HttpMethod = HttpMethods.POST,
                    BodyType = RequestBodyType.Form,
                    FormEncoding = FormEncoding.UrlEncoded
                };

                request.FormData.Add(new FormDataModel("description", "sample"));
                request.FormData.Add(new FormDataModel("file", string.Empty)
                {
                    FilePath = tempFile
                });

                var service = new HttpRequestService();
                using var content = service.CreateContent(null, request);

                var body = await content.ReadAsStringAsync();

                Assert.Contains("description=sample", body);
                Assert.DoesNotContain("file=", body);
                Assert.DoesNotContain("file-content", body);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
