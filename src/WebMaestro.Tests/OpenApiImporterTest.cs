using System.IO;
using System.Linq;
using System.Text;
using WebMaestro.Importers;
using Xunit;

namespace WebMaestro.Tests
{
    public class OpenApiImporterTest
    {
        [Fact]
        public void ImportShouldIncludePathLevelAndOperationLevelParameters()
        {
            var content = """
                openapi: 3.0.1
                info:
                  title: Parameter Import Test
                  version: 1.0.0
                servers:
                  - url: https://api.example.com
                paths:
                  /pets/{id}:
                    parameters:
                      - in: query
                        name: includeDetails
                        schema:
                          type: string
                      - in: header
                        name: X-Trace-Id
                        schema:
                          type: string
                    get:
                      operationId: getPet
                      parameters:
                        - in: query
                          name: locale
                          schema:
                            type: string
                      responses:
                        '200':
                          description: ok
                """;

            var importer = new OpenApiImporter();

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            importer.Import(stream);

            var request = Assert.Single(importer.Requests);

            Assert.Contains(request.QueryParams, x => x.Key == "includeDetails");
            Assert.Contains(request.QueryParams, x => x.Key == "locale");
            Assert.Contains(request.Headers, x => x.Name == "X-Trace-Id");
        }
    }
}
