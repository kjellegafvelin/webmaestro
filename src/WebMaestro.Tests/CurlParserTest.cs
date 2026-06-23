using System.Threading.Tasks;
using WebMaestro.Importers;
using WebMaestro.Models;
using WebMaestro.ViewModels;
using Xunit;

namespace WebMaestro.Tests
{
    public class CurlParserTest
    {
        [Fact]
        public void Parse_SimpleGet_ReturnsGetWithUrl()
        {
            var result = CurlParser.Parse("curl https://example.com");

            Assert.Equal(HttpMethods.GET, result.HttpMethod);
            Assert.Equal("https://example.com", result.Url);
            Assert.Empty(result.Headers);
            Assert.Empty(result.QueryParams);
        }

        [Fact]
        public void Parse_PostWithDataAndHeader_ReturnsCorrectModel()
        {
            var input = "curl -X POST -H 'Content-Type: application/json' -d '{\"key\":\"value\"}' https://example.com/api";

            var result = CurlParser.Parse(input);

            Assert.Equal(HttpMethods.POST, result.HttpMethod);
            Assert.Equal("https://example.com/api", result.Url);
            Assert.Single(result.Headers);
            Assert.Equal("Content-Type", result.Headers[0].Name);
            Assert.Equal("application/json", result.Headers[0].Value);
            Assert.Equal("{\"key\":\"value\"}", result.Body);
            Assert.Equal(RequestBodyType.Raw, result.BodyType);
        }

        [Fact]
        public void Parse_MultilinePostWithHeadersAndFormFields_ReturnsCorrectModel()
        {
            var input = """
                curl -X 'POST' \
                  'http://localhost:8080/v1/audio/transcriptions' \
                  -H 'accept: application/json' \
                  -H 'Content-Type: multipart/form-data' \
                  -F 'model=kb-whisper-base-x' \
                  -F 'file=@02-Jag-heter-Daniel.mp3;type=audio/mpeg'
                """;

            var result = CurlParser.Parse(input);

            Assert.Equal(HttpMethods.POST, result.HttpMethod);
            Assert.Equal("http://localhost:8080/v1/audio/transcriptions", result.Url);
            // Content-Type header should be removed since it's handled by FormEncoding
            Assert.Single(result.Headers);
            Assert.Equal("accept", result.Headers[0].Name);
            Assert.Equal("application/json", result.Headers[0].Value);
            Assert.Equal(RequestBodyType.Form, result.BodyType);
            Assert.Equal(FormEncoding.Multipart, result.FormEncoding);
            Assert.Equal(2, result.FormData.Count);
            Assert.Equal("model", result.FormData[0].Name);
            Assert.Equal("kb-whisper-base-x", result.FormData[0].Value);
            Assert.Equal("file", result.FormData[1].Name);
            Assert.Equal("@02-Jag-heter-Daniel.mp3;type=audio/mpeg", result.FormData[1].Value);
        }

        [Fact]
        public void Parse_UrlWithQueryString_PopulatesQueryParams()
        {
            var input = "curl 'https://example.com/search?q=hello&page=2'";

            var result = CurlParser.Parse(input);

            Assert.Equal("https://example.com/search", result.Url);
            Assert.Equal(2, result.QueryParams.Count);
            Assert.Equal("q", result.QueryParams[0].Key);
            Assert.Equal("hello", result.QueryParams[0].Value);
            Assert.Equal("page", result.QueryParams[1].Key);
            Assert.Equal("2", result.QueryParams[1].Value);
        }

        [Fact]
        public void Parse_BasicAuth_SetsAuthentication()
        {
            var input = "curl -u alice:s3cr3t https://example.com/secure";

            var result = CurlParser.Parse(input);

            Assert.Equal(AuthenticationTypes.Basic, result.Authentication.Type);
            Assert.Equal("alice", result.Authentication.Username);
            Assert.Equal("s3cr3t", result.Authentication.Password);
        }

        [Fact]
        public void Parse_ExplicitDeleteMethod_ReturnsDelete()
        {
            var input = "curl -X DELETE https://example.com/resource/42";

            var result = CurlParser.Parse(input);

            Assert.Equal(HttpMethods.DELETE, result.HttpMethod);
            Assert.Equal("https://example.com/resource/42", result.Url);
        }
    }
}
