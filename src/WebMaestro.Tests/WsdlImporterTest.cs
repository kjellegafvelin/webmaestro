using System.Reflection;
using System.Threading.Tasks;
using VerifyXunit;
using WebMaestro.Importers;
using Xunit;

namespace WebMaestro.Tests
{
    public class WsdlImporterTest : VerifyBase
    {
        public WsdlImporterTest() : base()
        {

        }

        [Fact]
        public Task ImportSample1()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var importer = new WsdlImporter();

            using (var stream = assembly.GetManifestResourceStream("WebMaestro.Tests.Resources.Wsdl Sample 1.xml"))
            {
                importer.Import(stream);
            }

            return Verify(importer);
        }

        [Fact]
        public Task ImportSample2()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var importer = new WsdlImporter();

            using (var stream = assembly.GetManifestResourceStream("WebMaestro.Tests.Resources.Wsdl Sample 2.xml"))
            {
                importer.Import(stream);
            }

            return Verify(importer);
        }

        [Fact]
        public Task ImportSample3()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var importer = new WsdlImporter();

            using (var stream = assembly.GetManifestResourceStream("WebMaestro.Tests.Resources.Wsdl Sample 3.xml"))
            {
                importer.Import(stream);
            }

            return Verify(importer);
        }

    }
}
