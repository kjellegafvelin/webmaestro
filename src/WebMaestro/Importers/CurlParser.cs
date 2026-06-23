using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebMaestro.Models;
using WebMaestro.ViewModels;

namespace WebMaestro.Importers
{
    internal static class CurlParser
    {
        public static RequestModel Parse(string curlInput)
        {
            var normalized = NormalizeMultiline(curlInput);
            var tokens = Tokenize(normalized);

            var model = new RequestModel();
            string method = null;
            string url = null;
            var formFields = new List<string>();
            var tempHeaders = new List<(string Name, string Value)>();
            bool forceGet = false;

            for (int i = 1; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token)
                {
                    case "-X":
                    case "--request":
                        method = tokens[++i];
                        break;
                    case "-H":
                    case "--header":
                        var headerRaw = tokens[++i];
                        var colonIdx = headerRaw.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            var name = headerRaw[..colonIdx].Trim();
                            var val = headerRaw[(colonIdx + 1)..].Trim();
                            tempHeaders.Add((name, val));
                        }
                        break;
                    case "-d":
                    case "--data":
                    case "--data-raw":
                    case "--data-ascii":
                        model.Body = tokens[++i];
                        model.BodyType = RequestBodyType.Raw;
                        break;
                    case "-F":
                    case "--form":
                        formFields.Add(tokens[++i]);
                        break;
                    case "-u":
                    case "--user":
                        var userpass = tokens[++i];
                        var colonPos = userpass.IndexOf(':');
                        if (colonPos > 0)
                        {
                            model.Authentication.Type = AuthenticationTypes.Basic;
                            model.Authentication.Username = userpass[..colonPos];
                            model.Authentication.Password = userpass[(colonPos + 1)..];
                        }
                        break;
                    case "-G":
                    case "--get":
                        forceGet = true;
                        break;
                    case "--url":
                        url = tokens[++i];
                        break;
                    default:
                        if (!token.StartsWith('-') && url is null)
                        {
                            url = token;
                        }
                        break;
                }
            }

            if (forceGet)
            {
                model.HttpMethod = HttpMethods.GET;
            }
            else if (method is not null && Enum.TryParse<HttpMethods>(method, true, out var parsed))
            {
                model.HttpMethod = parsed;
            }
            else if (model.Body is not null || formFields.Count > 0)
            {
                model.HttpMethod = HttpMethods.POST;
            }

            if (url is not null)
            {
                var qIdx = url.IndexOf('?');
                if (qIdx >= 0)
                {
                    model.Url = url[..qIdx];
                    var pairs = HttpUtility.ParseQueryString(url[(qIdx + 1)..]);
                    foreach (string key in pairs)
                    {
                        if (key is not null)
                        {
                            model.QueryParams.Add(new QueryParamModel(key, pairs[key] ?? string.Empty));
                        }
                    }
                }
                else
                {
                    model.Url = url;
                }
            }

            // Process form fields and determine encoding
            if (formFields.Count > 0)
            {
                model.BodyType = RequestBodyType.Form;

                // Check if Content-Type header specifies multipart/form-data
                var contentTypeHeader = tempHeaders.FirstOrDefault(h => 
                    h.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase));

                if (contentTypeHeader.Value?.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase) == true)
                {
                    model.FormEncoding = FormEncoding.Multipart;
                    // Remove the Content-Type header since it will be auto-generated
                    tempHeaders.Remove(contentTypeHeader);
                }
                else
                {
                    model.FormEncoding = FormEncoding.UrlEncoded;
                }

                foreach (var field in formFields)
                {
                    ParseFormField(field, model);
                }
            }

            // Add headers to model (after filtering out form-related ones)
            foreach (var (name, value) in tempHeaders)
            {
                model.Headers.Add(new HeaderModel(name, value));
            }

            return model;
        }

        private static string NormalizeMultiline(string input)
        {
            var lines = input.Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmed = line.TrimEnd();
                if (trimmed.EndsWith('\\'))
                {
                    sb.Append(trimmed[..^1]);
                    sb.Append(' ');
                }
                else
                {
                    sb.Append(trimmed);
                }
            }
            return sb.ToString().Trim();
        }

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var current = new StringBuilder();
            var i = 0;
            while (i < input.Length)
            {
                var c = input[i];
                if (c == '\'' || c == '"')
                {
                    var quote = c;
                    i++;
                    while (i < input.Length && input[i] != quote)
                    {
                        current.Append(input[i]);
                        i++;
                    }
                    i++;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    i++;
                }
                else
                {
                    current.Append(c);
                    i++;
                }
            }
            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }
            return tokens;
        }

        private static void ParseFormField(string field, RequestModel model)
        {
            // Form field format: name=value or name=@filename
            // For simplicity, we parse basic key=value pairs
            var eqIdx = field.IndexOf('=');
            if (eqIdx > 0)
            {
                var name = field[..eqIdx];
                var value = field[(eqIdx + 1)..];

                // Skip file uploads (@filename) for now, just store the reference
                model.FormData.Add(new FormDataModel { Name = name, Value = value, IsEnabled = true });
            }
            else if (!string.IsNullOrWhiteSpace(field))
            {
                // If no equals sign, treat as a flag/checkbox-like field
                model.FormData.Add(new FormDataModel { Name = field, Value = string.Empty, IsEnabled = true });
            }
        }
    }
}
