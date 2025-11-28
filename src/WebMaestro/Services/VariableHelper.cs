using System;
using System.Collections.Generic;
using WebMaestro.Models;

namespace WebMaestro.Services
{
    internal static class VariableHelper
    {
        public static string ApplyVariables(EnvironmentModel environment, RequestModel request, string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (environment?.Variables != null)
            {
                foreach (var v in environment.Variables)
                {
                    if (!merged.ContainsKey(v.Name)) merged[v.Name] = v.Value ?? string.Empty;
                }
            }
            if (request?.Variables != null)
            {
                foreach (var v in request.Variables)
                {
                    merged[v.Name] = v.Value ?? string.Empty;
                }
            }
            foreach (var kv in merged)
            {
                value = value.Replace($"${{{kv.Key}}}", kv.Value, StringComparison.OrdinalIgnoreCase);
            }
            return value;
        }
    }
}
