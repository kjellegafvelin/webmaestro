using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebMaestro.Models;
using WebMaestro.Serializers;
using WebMaestro.Services;
using Xunit;
using System.Collections.ObjectModel;

namespace WebMaestro.Tests
{
    public class RequestModelSerializerTest
    {
        [Fact]
        public async Task Deserialize_Single_Request_Test()
        {
            // Arrange
            using Stream stream = File.OpenRead("Resources/Single_Request.http");

            // Act
            List<RequestModel> requests = await RequestModelSerializer.DeserializeAsync(stream);

            // Assert
            Assert.Single(requests);
            Assert.Equal("GET", requests[0].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values", requests[0].Url);
            Assert.Empty(requests[0].Variables);
            Assert.Empty(requests[0].Headers);
            Assert.Equal(0, requests[0].Body.Length);
        }

        [Fact]
        public async Task Deserialize_Single_Request_Headers_Test()
        {
            // Arrange
            using Stream stream = File.OpenRead("Resources/Single_Request_Headers.http");

            // Act
            List<RequestModel> requests = await RequestModelSerializer.DeserializeAsync(stream);

            // Assert
            Assert.Single(requests);
            Assert.Equal("GET", requests[0].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values", requests[0].Url);
            Assert.Empty(requests[0].Variables);
            Assert.Equal(2, requests[0].Headers.Count);
            Assert.Equal(0, requests[0].Body.Length);
        }

        [Fact]
        public async Task Deserialize_Single_Request_Headers_Variables_Test()
        {
            // Arrange
            using Stream stream = File.OpenRead("Resources/Single_Request_Headers_Variables.http");

            // Act
            List<RequestModel> requests = await RequestModelSerializer.DeserializeAsync(stream);

            // Assert
            Assert.Single(requests);
            Assert.Equal("GET", requests[0].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values/${{test}}", requests[0].Url);
            Assert.Equal(2, requests[0].Variables.Count);
            Assert.Equal(2, requests[0].Headers.Count);
            Assert.Equal("${{test2}}", requests[0].Headers[1].Value);
            Assert.Equal(0, requests[0].Body.Length);
        }

        [Fact]
        public async Task Deserialize_Single_Request_Headers_Variables_Body_Test()
        {
            // Arrange
            using Stream stream = File.OpenRead("Resources/Single_Request_Headers_Variables_Body.http");

            // Act
            List<RequestModel> requests = await RequestModelSerializer.DeserializeAsync(stream);

            // Assert
            Assert.Single(requests);
            Assert.Equal("POST", requests[0].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values/${{test}}", requests[0].Url);
            Assert.Equal(2, requests[0].Variables.Count);
            Assert.Equal(2, requests[0].Headers.Count);
            Assert.Equal("${{test2}}", requests[0].Headers[1].Value);
            Assert.Equal(46, requests[0].Body.Length);
            Assert.Contains("${{test}}", requests[0].Body);
        }

        [Fact]
        public async Task Deserialize_Multiple_Request_Headers_Variables_Body_Test()
        {
            // Arrange
            using Stream stream = File.OpenRead("Resources/Multiple_Requests_Headers_Variables_Body.http");

            // Act
            List<RequestModel> requests = await RequestModelSerializer.DeserializeAsync(stream);

            // Assert
            Assert.Equal(2, requests.Count);
            
            Assert.Equal("POST", requests[0].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values/${{test}}", requests[0].Url);
            Assert.Equal(2, requests[0].Variables.Count);
            Assert.Equal(2, requests[0].Headers.Count);
            Assert.Equal("${{test2}}", requests[0].Headers[1].Value);
            Assert.Equal(48, requests[0].Body.Length);
            Assert.Contains("${{test}}", requests[0].Body);

            Assert.Equal("DELETE", requests[1].HttpMethod.ToString());
            Assert.Equal("https://localhost:5001/api/values/", requests[1].Url);
            Assert.Empty(requests[1].Variables);
            Assert.Empty(requests[1].Headers);
            Assert.Empty(requests[1].Body);
        }

        [Fact]
        public void ApplyVariables_Environment_And_Request_Merge_Test()
        {
            var env = new EnvironmentModel()
            {
                Name = "env1",
                Variables = new ObservableCollection<VariableModel>
                {
                    new VariableModel("baseUrl", "https://api.example.com", string.Empty, true),
                    new VariableModel("token", "env-token", string.Empty, true),
                }
            };

            var req = new RequestModel();
            req.Variables.Add(new VariableModel("token", "req-token", string.Empty, true));
            req.Variables.Add(new VariableModel("id", "123", string.Empty, true));

            var result = VariableHelper.ApplyVariables(env, req, "${baseUrl}/items/${id}?auth=${token}");

            Assert.Equal("https://api.example.com/items/123?auth=req-token", result);
        }
    }
}