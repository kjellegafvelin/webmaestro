using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VerifyTests;
using VerifyXunit;
using WebMaestro.Importers;
using Xunit;

namespace WebMaestro.Tests
{
    [UsesVerify]
    public class RawHttpImporterTest: VerifyBase
    {
        public RawHttpImporterTest() : base()
        {
            ClipboardAccept.Enable();
        }

        [Fact]
        public Task ImportGetTest()
        {
            var data = LoadData("Http_Get.txt");

            var importer = new RawHttpImporter();
            var req = importer.Import(data);

            return Verify(req);
        }

        [Fact]
        public Task ImportGetTest_FullUrl()
        {
            var data = LoadData("Http_Get_FullUrl.txt");

            var importer = new RawHttpImporter();
            var req = importer.Import(data);

            return Verify(req);
        }

        [Fact]
        public Task ImportPostTest()
        {
            var data = LoadData("Http_Post.txt");

            var importer = new RawHttpImporter();
            var req = importer.Import(data);

            return Verify(req);
        }

        private static string LoadData(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream($"WebMaestro.Tests.Resources.{filename}"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
