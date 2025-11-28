using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Services
{
    internal class HttpRequestService
    {

        public async Task<ResponseModel> SendAsync(EnvironmentModel environment, RequestModel request, CancellationToken cancellationToken)
        {
            var handler = new SocketsHttpHandler();
            //handler.AllowAutoRedirect = this.allowAutoRedirect;

            var client = new HttpClient(handler, false);

            try
            {
                var response = new ResponseModel();

                var url = CreateUrl(environment, request);

                AddAuthentication(environment, request, client.DefaultRequestHeaders, ref url);

                if (!request.HttpsProtocols.UseDefault)
                {
                    if (request.HttpsProtocols.UseTls10)
                    {
#pragma warning disable SYSLIB0039 // Type or member is obsolete
                        handler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls;
#pragma warning restore SYSLIB0039 // Type or member is obsolete
                    }
                    if (request.HttpsProtocols.UseTls11)
                    {
#pragma warning disable SYSLIB0039 // Type or member is obsolete
                        handler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls11;
#pragma warning restore SYSLIB0039 // Type or member is obsolete
                    }
                    if (request.HttpsProtocols.UseTls12)
                    {
                        handler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls12;
                    }
                    if (request.HttpsProtocols.UseTls13)
                    {
                        handler.SslOptions.EnabledSslProtocols |= SslProtocols.Tls13;
                    }
                }
                else
                {
                    handler.SslOptions.EnabledSslProtocols = SslProtocols.None;
                }

                client.Timeout = TimeSpan.FromSeconds(request.Timeout);

                X509Certificate2 serverCertificate = null;

                handler.SslOptions.RemoteCertificateValidationCallback += (object _, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    serverCertificate = new X509Certificate2(certificate.GetRawCertData());

                    //if (!this.validateServerCertificate)
                    //{
                    //    return true;
                    //}

                    return sslPolicyErrors == SslPolicyErrors.None;
                };

                var contentType = string.Empty;

                foreach (var header in request.Headers.Where(x => x.IsEnabled))
                {
                    var processedValue = VariableHelper.ApplyVariables(environment, request, header.Value);

                    switch (header.Name.ToLower())
                    {
                        case "accept":
                            client.DefaultRequestHeaders.Accept.ParseAdd(processedValue);
                            break;
                        case "connection":
                        case "content-length":
                        case "date":
                            // Values are set automatically
                            break;
                        case "content-type":
                            contentType = processedValue;
                            break;
                        case "expect":
                            client.DefaultRequestHeaders.Expect.ParseAdd(processedValue);
                            break;
                        case "host":
                            client.DefaultRequestHeaders.Host = processedValue;
                            break;
                        case "if-modified-since":
                            client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.Parse(processedValue);
                            break;
                        case "referer":
                            client.DefaultRequestHeaders.Referrer = new Uri(processedValue);
                            break;
                        case "user-agent":
                            client.DefaultRequestHeaders.UserAgent.ParseAdd(processedValue);
                            break;
                        default:
                            client.DefaultRequestHeaders.Add(header.Name, processedValue);
                            break;
                    }
                }

                // Decided to set cookies in the header instead to not poison
                // the cookiecontainer when the cookies from the response is extracted
                if (request.Cookies.Count > 0)
                {
                    var cookiesSB = new StringBuilder();
                    var delimiter = string.Empty;

                    foreach (var cookie in request.Cookies)
                    {
                        var val = VariableHelper.ApplyVariables(environment, request, cookie.Value);
                        cookiesSB.Append(delimiter);
                        cookiesSB.Append($"{cookie.Name}={val}");
                        delimiter = "; ";
                    }

                    client.DefaultRequestHeaders.Add("cookie", cookiesSB.ToString());
                }

                handler.SslOptions.ClientCertificates = new(request.Certificates.ToArray());

                HttpRequestMessage msg = null;

                switch (request.HttpMethod)
                {
                    case HttpMethods.HEAD:
                        msg = new HttpRequestMessage(HttpMethod.Head, url);
                        break;
                    case HttpMethods.GET:
                        msg = new HttpRequestMessage(HttpMethod.Get, url);
                        break;
                    case HttpMethods.POST:
                    case HttpMethods.PATCH:
                    case HttpMethods.PUT:
                        msg = new HttpRequestMessage(new HttpMethod(request.HttpMethod.ToString()), url)
                        {
                            Content = CreateContent(environment, request)
                        };

                        contentType = msg.Content is StreamContent ? "application/octet-stream" : contentType;

                        if (!string.IsNullOrWhiteSpace(contentType))
                        {
                            msg.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        }
                        break;
                }

                msg.Version = request.HttpVersion switch
                {
                    Models.HttpVersion.HTTP11 => new Version(1, 1),
                    Models.HttpVersion.HTTP20 => new Version(2, 0),
                    Models.HttpVersion.AUTO => msg.Version,
                    _ => msg.Version
                };

                if (request.HttpVersion != Models.HttpVersion.AUTO)
                {
                    msg.VersionPolicy = request.HttpVersionExact ? HttpVersionPolicy.RequestVersionExact : HttpVersionPolicy.RequestVersionOrLower;
                }


                var sw = Stopwatch.StartNew();
                HttpResponseMessage httpResponse;

                try
                {
                    httpResponse = await client.SendAsync(msg, cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException is AuthenticationException authEx)
                        {
                            OnError("The SSL connection could not be established. The remote certificate is invalid.");
                        }
                        else if (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            OnError($"Could not connect to {request.Url}. Connection was refused. Please check that the url is valid and that the endpoint is up and running.");
                        }
                    }

                    throw;
                }
                catch (WebException ex)
                {
                    if (ex.InnerException != null && ex.InnerException is SocketException innerEx && innerEx.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        OnError($"Could not connect to {request.Url}. Connection was refused. Please check that the url is valid and that the endpoint is up and running.");
                    }

                    throw;
                }

                sw.Stop();

                response.Status = httpResponse.StatusCode;
                response.Elapsed = sw.Elapsed;
                response.ContentType = httpResponse.Content.Headers.ContentType?.ToString();
                response.Url = httpResponse.RequestMessage.RequestUri.ToString();
                response.Reason = httpResponse.ReasonPhrase;
                response.HttpVersion = httpResponse.Version.ToString(2);

                foreach (Cookie cookie in handler.CookieContainer.GetAllCookies())
                {
                    response.Cookies.Add(new CookieModel()
                    {
                        Name = cookie.Name,
                        Value = cookie.Value,
                        Path = cookie.Path,
                        Domain = cookie.Domain,
                        Expires = cookie.Expires,
                        HttpOnly = cookie.HttpOnly,
                        Secure = cookie.Secure
                    });
                }

                foreach (var header in httpResponse.Headers)
                {
                    response.Headers.Add(new HeaderModel(header.Key, string.Join(",", header.Value)));
                }

                foreach (var header in httpResponse.Content.Headers)
                {
                    response.Headers.Add(new HeaderModel(header.Key, string.Join(",", header.Value)));
                }

                if (request.HttpMethod != HttpMethods.HEAD && request.HttpMethod != HttpMethods.TRACE)
                {
                    if (httpResponse.Content.Headers.ContentLength > 50 * 1024 * 1024)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("=================================");
                        sb.AppendLine("The response is larger than 50 MB");
                        sb.AppendLine("=================================");
                        response.Body = sb.ToString();
                    }
                    else
                    {
                        var result = await GetBody(httpResponse);

                        if (result != null)
                        {
                            response.Body = result;
                            response.Size = System.Text.UTF8Encoding.UTF8.GetByteCount(result);
                        }
                    }
                }

                if (serverCertificate != null)
                {
                    response.ServerCertificate = serverCertificate;
                }

                return response;
            }
            catch (WebException ex)
            {
                OnError("Connection failure: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                OnError("Unknown error: " + ex.Message);
                throw; //TODO: Fix proper error handling...
            }
            finally
            {
                client.Dispose();
            }

        }

        private Uri CreateUrl(EnvironmentModel environment, RequestModel request)
        {
            var url = request.Url;
            url = VariableHelper.ApplyVariables(environment, request, url);
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new Exception("The request URL is not an absolute URL. Use variables to provide a full URL.");
            }
            return new Uri(url);
        }

        private HttpContent CreateContent(EnvironmentModel environment, RequestModel request)
        {
            switch (request.BodyType)
            {
                case RequestBodyType.Form:
                    return null;
                case RequestBodyType.Raw:
                    var body = request.Body ?? "";

                    body = VariableHelper.ApplyVariables(environment, request, body);

                    return new StringContent(body, Encoding.UTF8);
                case RequestBodyType.Binary:
                    FileStream file = File.OpenRead(request.Filename);
                    return new StreamContent(file);
                default:
                    return null;
            }
        }

        private void AddAuthentication(EnvironmentModel environment, RequestModel request, HttpRequestHeaders headers, ref Uri url)
        {
            var auth = request.Authentication;

            switch (auth.Type)
            {
                case AuthenticationTypes.Basic:
                    var username = VariableHelper.ApplyVariables(environment, request, auth.Username);
                    var password = VariableHelper.ApplyVariables(environment, request, auth.Password);
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    break;
                case AuthenticationTypes.ApiKey:
                    switch (auth.ApiKeyLocation)
                    {
                        case ApiKeyLocations.Header:
                            headers.Add(auth.Key, VariableHelper.ApplyVariables(environment, request, auth.Value));
                            break;
                        case ApiKeyLocations.Querystring:
                            url = url.AddQueryParam(auth.Key, VariableHelper.ApplyVariables(environment, request, auth.Value));
                            break;
                    }
                    break;
                case AuthenticationTypes.BearerToken:
                    headers.Authorization = new AuthenticationHeaderValue("Bearer", VariableHelper.ApplyVariables(environment, request, auth.Token));
                    break;
            }
        }

        private static async Task<string> GetBody(HttpResponseMessage response)
        {
            var result = string.Empty;

            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            {
                if (responseStream.Length == 0)
                {
                    return string.Empty;
                }

                Stream tempStream = null;

                if (response.Content.Headers.ContentEncoding.Any(x => x.Equals("gzip", StringComparison.OrdinalIgnoreCase)))
                {
                    tempStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }
                else if (response.Content.Headers.ContentEncoding.Any(x => x.Equals("deflate", StringComparison.OrdinalIgnoreCase)))
                {
                    tempStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                }
                else
                {
                    tempStream = responseStream;
                }

                using (var reader = new StreamReader(tempStream, Encoding.UTF8))
                {
                    result = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                tempStream.Dispose();
            }

            return result;
        }

        internal static void OnError(string msg)
        {
            throw new Exception(msg);
        }


    }
}
