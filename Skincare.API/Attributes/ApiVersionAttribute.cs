using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using System;

namespace Skincare.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiVersionAttribute : Attribute
    {
        public string Version { get; }
        public bool IsDeprecated { get; }

        public ApiVersionAttribute(string version, bool isDeprecated = false)
        {
            Version = version;
            IsDeprecated = isDeprecated;
        }
    }

    public static class ApiVersionExtensions
    {
        public static void AddApiVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            var apiVersioningConfig = configuration.GetSection("ApiVersioning");
            var defaultVersion = apiVersioningConfig["DefaultVersion"] ?? "1.0";
            var supportedVersions = apiVersioningConfig.GetSection("SupportedVersions").Get<string[]>() ?? new[] { "1.0" };
            var deprecatedVersions = apiVersioningConfig.GetSection("DeprecatedVersions").Get<string[]>() ?? new string[0];
            var versionHeaderName = apiVersioningConfig["VersionHeaderName"] ?? "X-API-Version";
            var versionQueryParameterName = apiVersioningConfig["VersionQueryParameterName"] ?? "api-version";

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersion.Parse(defaultVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader(versionHeaderName),
                    new QueryStringApiVersionReader(versionQueryParameterName),
                    new UrlSegmentApiVersionReader()
                );
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }
    }
} 