using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.IO;
using System.Linq;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Importers
{
    internal class OpenApiImporter : Importer
    {
        private readonly string baseUrl;

        public OpenApiImporter()
        {
        }

        public OpenApiImporter(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public override void Import(Stream stream)
        {
            var openApi = new OpenApiStreamReader().Read(stream, out var diagnostic);

            var isOpenApi2 = diagnostic.SpecificationVersion == Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;

            this.Collection.Name = openApi.Info.Title;

            if (isOpenApi2)
            {
                openApi.Extensions.TryGetValue("x-servers", out var servers);

                if (servers is not null)
                {
                    var basePath = new Uri(openApi.Servers.First().Url).AbsolutePath;
                    
                    var serverList = (OpenApiArray)servers;

                    foreach (var server in serverList)
                    {
                        var url = ((OpenApiObject)server).TryGetValue("url", out var urlTemp) 
                            ? (urlTemp as OpenApiString).Value : string.Empty;

                        var description = ((OpenApiObject)server).TryGetValue("description", out var descTemp)
                            ? (descTemp as OpenApiString).Value : url;


                        var env = new EnvironmentModel()
                        {
                            Name = description ,
                            Url = url + basePath
                        };

                        this.Collection.Environments.Add(env);
                    }
                }
                else
                {
                    var env = new EnvironmentModel()
                    {
                        Name = openApi.Info.Title,
                        Url = baseUrl
                    };

                    this.Collection.Environments.Add(env);
                }
            }
            else
            {
                foreach (var server in openApi.Servers)
                {
                    var url = server.Url;

                    if (url.StartsWith('/'))
                    {
                        url = baseUrl + url;
                    }

                    url = url.Replace("{", "${");

                    var env = new EnvironmentModel()
                    {
                        Name = server.Description ?? url,
                        Url = url
                    };

                    foreach ((string key, OpenApiServerVariable value) in server.Variables)
                    {
                        var variable = new VariableModel(key, value.Default, value.Description);
                        env.Variables.Add(variable);
                    }

                    this.Collection.Environments.Add(env);
                }
            }

            if (this.Collection.Environments.Count == 0)
            {
                var env = new EnvironmentModel()
                {
                    Name = openApi.Info.Title,
                    Url = baseUrl
                };

                this.Collection.Environments.Add(env);
            }

            foreach (var (path, pathInfo) in openApi.Paths)
            {
                foreach (var (operationType, operationInfo) in pathInfo.Operations)
                {
                    if (operationInfo.Deprecated)
                    {
                        continue;
                    }

                    var req = new RequestModel()
                    {
                        Name = operationInfo.Summary ?? $"{operationType.ToString().ToLowerInvariant()}-{path.Replace('/', '-').ToLowerInvariant()}",
                        Url = path.Replace("{", "${"),
                        HttpMethod = ConvertToHttpMethod(operationType)
                    };

                    foreach (var parameter in operationInfo.Parameters)
                    {
                        if (parameter.Deprecated)
                        {
                            continue;
                        }

                        switch (parameter.In)
                        {
                            case ParameterLocation.Query:
                                req.QueryParams.Add(new(parameter.Name, string.Empty, parameter.Description));
                                break;
                            case ParameterLocation.Header:
                                var value = GetDefaultValueForHeader(parameter);
                                var header = new HeaderModel(parameter.Name, value, parameter.Description)
                                {
                                    IsEnabled = parameter.Required
                                };

                                req.Headers.Add(header);
                                break;
                            case ParameterLocation.Path:
                                req.Variables.Add(new(parameter.Name, string.Empty, parameter.Description, parameter.Required));
                                break;
                            case ParameterLocation.Cookie:
                                break;
                            default:
                                break;
                        }
                    }

                    base.Requests.Add(req);
                }
            }
        }

        private static string GetDefaultValueForHeader(OpenApiParameter parameter)
        {
            var value = string.Empty;
            if (parameter.Schema.Default is not null)
            {
                var defaultValue = parameter.Schema.Default;

                value = defaultValue switch
                {
                    OpenApiString => ((OpenApiString)defaultValue).Value,
                    OpenApiArray => string.Join(',', (OpenApiArray)defaultValue),
                    OpenApiObject => GetValueForHeader((OpenApiObject)defaultValue, parameter.Explode),
                    _ => string.Empty
                };
            }

            return value;
        }

        private static string GetValueForHeader(OpenApiObject defaultValue, bool shouldExplode)
        {
            var joiner = shouldExplode ? "=" : ",";

            return string.Join(',', defaultValue.Select(x => x.Key + joiner + x.Value));
        }

        private static HttpMethods ConvertToHttpMethod(OperationType operationType)
        {
            var method = operationType switch
            {
                OperationType.Get => HttpMethods.GET,
                OperationType.Patch => HttpMethods.PATCH,
                OperationType.Post => HttpMethods.POST,
                OperationType.Delete => HttpMethods.DELETE,
                OperationType.Head => HttpMethods.HEAD,
                OperationType.Options => HttpMethods.OPTIONS,
                OperationType.Put => HttpMethods.PUT,
                OperationType.Trace => HttpMethods.TRACE,
                _ => HttpMethods.GET
            };

            return method;
        }

    }
}
