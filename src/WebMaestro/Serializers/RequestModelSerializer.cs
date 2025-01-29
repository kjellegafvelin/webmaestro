using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Serializers;

public class RequestModelSerializer
{
    private const string HTTP_VARIABLE_START = "{{";
    private const string WM_VARIABLE_START = "${{";

    public static async Task SerializeAsync(RequestModel requestModel, Stream stream)
    {
        var httpRequestMessage = CreateHttpRequestMessage(requestModel);
        await SerializeHttpRequestMessageAsync(httpRequestMessage, stream);
    }

    private static HttpRequestMessage CreateHttpRequestMessage(RequestModel requestModel)
    {
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(requestModel.HttpMethod.ToString()),
            RequestUri = new Uri(requestModel.Url),
            Content = new StringContent(requestModel.Body ?? string.Empty, Encoding.UTF8, requestModel.ContentType)
        };

        foreach (var header in requestModel.Headers)
        {
            if (header.IsEnabled)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }
        }

        if (requestModel.Authentication.Type != AuthenticationTypes.None)
        {
            AddAuthentication(httpRequestMessage, requestModel.Authentication);
        }

        return httpRequestMessage;
    }

    private static void AddAuthentication(HttpRequestMessage httpRequestMessage, Authentication authentication)
    {
        switch (authentication.Type)
        {
            case AuthenticationTypes.Basic:
                var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authentication.Username}:{authentication.Password}"));
                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthValue);
                break;
            case AuthenticationTypes.BearerToken:
                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authentication.Token);
                break;
            case AuthenticationTypes.ApiKey:
                if (authentication.ApiKeyLocation == ApiKeyLocations.Header)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(authentication.Key, authentication.Value);
                }
                else if (authentication.ApiKeyLocation == ApiKeyLocations.Querystring)
                {
                    var uriBuilder = new UriBuilder(httpRequestMessage.RequestUri!);
                    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                    query[authentication.Key] = authentication.Value;
                    uriBuilder.Query = query.ToString();
                    httpRequestMessage.RequestUri = uriBuilder.Uri;
                }
                break;
        }
    }

    private static async Task SerializeHttpRequestMessageAsync(HttpRequestMessage httpRequestMessage, Stream stream)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        // Request line
        await writer.WriteLineAsync($"{httpRequestMessage.Method} {httpRequestMessage.RequestUri!.PathAndQuery} HTTP/{httpRequestMessage.Version}");

        // Headers
        foreach (var header in httpRequestMessage.Headers)
        {
            await writer.WriteLineAsync($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (httpRequestMessage.Content != null)
        {
            foreach (var header in httpRequestMessage.Content.Headers)
            {
                await writer.WriteLineAsync($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        await writer.WriteLineAsync();

        // Body
        if (httpRequestMessage.Content != null)
        {
            await writer.WriteLineAsync(await httpRequestMessage.Content.ReadAsStringAsync());
        }

        await writer.FlushAsync();
    }

    public static async Task<List<RequestModel>> DeserializeAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await DeserializeAsync(reader);
    }

    public static async Task<List<RequestModel>> DeserializeAsync(string rawHttp)
    {
        using StringReader reader = new(rawHttp);
        return await DeserializeAsync(reader);
    }

    private static async Task<List<RequestModel>> DeserializeAsync(TextReader reader)
    {
        List<RequestModel> requestModels = [];
        bool hasMoreRequests = false;

        // Parse requests
        do
        {
            string? line;
            RequestModel requestModel = new();
            hasMoreRequests = false;

            // Parse variables
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }
                else if (line.StartsWith('@'))
                {
                    string[] parts = line.Split('=', 2);
                    requestModel.Variables.Add(new(parts[0][1..], parts[1], string.Empty));
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
            }

            if (line == null)
            {
                throw new Exception("Request is empty");
            }

            // parse line where request starts
            var requestLine = line.Split(' ');

            var method = requestLine[0];
            var url = requestLine[1];
            string? httpVersion = requestLine.Length == 3 ? requestLine[2] : null;

            requestModel.HttpMethod = Enum.Parse<HttpMethods>(method, true);
            requestModel.Url = url.Replace(HTTP_VARIABLE_START, WM_VARIABLE_START);

            if (httpVersion != null)
            {
                requestModel.HttpVersion = Enum.Parse<HttpVersion>(httpVersion.Replace("HTTP/", ""), true);
            }

            // Parse headers
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var headerParts = line.Split([": "], 2, StringSplitOptions.None);
                if (headerParts.Length == 2)
                {
                    requestModel.Headers.Add(new HeaderModel(headerParts[0], headerParts[1].Replace(HTTP_VARIABLE_START, WM_VARIABLE_START)));
                }
            }

            // Parse body
            var bodyBuilder = new StringBuilder();
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("###"))
                {
                    hasMoreRequests = true;
                    break;
                }

                bodyBuilder.AppendLine(line);
            }

            requestModel.Body = bodyBuilder.ToString().Replace(HTTP_VARIABLE_START, WM_VARIABLE_START);

            requestModels.Add(requestModel);
        } while (hasMoreRequests);

        return requestModels;
    }
}
