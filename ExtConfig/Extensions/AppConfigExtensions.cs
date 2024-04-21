using System;
using Microsoft.Extensions.Configuration;

namespace ExtConfig.Extensions;

public static class AppConfigExtensions
{
    public static TAppConfig ToAppConfig<TAppConfig>(this IConfiguration configuration) where TAppConfig : class
    {
        var appConfigSection = configuration.GetSection(typeof(TAppConfig).Name);
        var appConfig = JsonConfigBuilder.Build<TAppConfig>(appConfigSection);
        if (appConfig == null)
            throw new ArgumentException(nameof(appConfig));
        return appConfig;
    }
}