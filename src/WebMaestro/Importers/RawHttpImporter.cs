using System;
using WebMaestro.Models;
using WebMaestro.ViewModels;
using System.Linq;
using System.Text;

namespace WebMaestro.Importers
{
    internal class RawHttpImporter
    {
        private RequestModel request;
        private string path;

        public RequestModel Import(ReadOnlySpan<char> rawHttp)
        {
            request = new RequestModel();

            if (rawHttp.Length == 0)
            {
                return null;
            }

            var enumerator = rawHttp.EnumerateLines();
            
            if (!enumerator.MoveNext())
            {
                return null;
            }

            ReadStartLine(enumerator.Current);

            while(enumerator.MoveNext())
            {
                // An empty line defines the end of the header section
                if (enumerator.Current.IsEmpty)
                {
                    break;
                }

                ReadHeaderLine(enumerator.Current);
            }

            
            if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                if (!uri.IsAbsoluteUri)
                {
                    var hostHeader = request.Headers.FirstOrDefault(x => x.Name.Equals("host", StringComparison.InvariantCultureIgnoreCase));

                    if (hostHeader != null)
                    {
                        // Assume that most requests will be done with https
                        uri = new Uri("https://" + hostHeader.Value + path);
                    }
                }

                request.Url = uri.ToString();
            }

            ExtractCookies();

            var bodySB = new StringBuilder();

            while (enumerator.MoveNext())
            {
                bodySB.AppendLine(enumerator.Current.ToString());
            }

            request.Body = bodySB.ToString();

            return request;
        }

        private void ReadStartLine(ReadOnlySpan<char> startLine)
        {
            var firstIndex = 0;
            var lastIndex = startLine.IndexOf(' ');

            request.HttpMethod = Enum.Parse<HttpMethods>(startLine[firstIndex..lastIndex]);

            firstIndex = lastIndex + 1;
            lastIndex = startLine.LastIndexOf(' ');

            path = startLine[firstIndex..lastIndex].ToString();
        }

        private void ReadHeaderLine(ReadOnlySpan<char> headerLine)
        {
            var header = new HeaderModel();

            var index = headerLine.IndexOf(':');

            header.Name = headerLine[0..index].ToString();
            header.Value = headerLine[(index + 1)..].TrimStart().ToString();
            
            request.Headers.Add(header);
        }

        private void ExtractCookies()
        {
            var cookieHeader = request.Headers.FirstOrDefault(x => x.Name.Equals("cookie", StringComparison.InvariantCultureIgnoreCase));

            if (cookieHeader == null)
            {
                return;
            }

            var cookies = cookieHeader.Value.Split(';');

            foreach (var cookie in cookies)
            {
                var index = cookie.IndexOf('=');
                var model = new CookieModel();
                model.Name = cookie[..index];
                model.Value = cookie[(index + 1)..];

                request.Cookies.Add(model);
            }
        }
    }
}
