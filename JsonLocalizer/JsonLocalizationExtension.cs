using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace JsonLocalizer;

/// <summary>
/// Extension methods to configure JSON-based localization in an ASP.NET Core application.
/// </summary>
public static class JsonLocalizationExtension
{
    /// <summary>
    /// Registers services required for JSON-based localization into the DI container.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add services to.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance for chaining.</returns>
    public static WebApplicationBuilder AddJsonLocalization(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalization();
        builder.Services.AddSingleton<LocalizationMiddleware>();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        return builder;
    }

    /// <summary>
    /// Configures the application to use JSON-based localization with custom request culture resolution.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <param name="supportedCultures">A list of supported <see cref="CultureInfo"/> instances.</param>
    /// <param name="defaultLanguage">The default language to use if no supported language is specified in the request.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseJsonLocalization(
        this IApplicationBuilder app,
        IList<CultureInfo> supportedCultures,
        string defaultLanguage)
    {
        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultLanguage),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders =
            [
                new CustomRequestCultureProvider(context =>
                {
                    var language = context.Request.Headers.AcceptLanguage.ToString();
                    var firstLang = language?.Split(',').FirstOrDefault();
                    var culture = supportedCultures.FirstOrDefault(c =>
                        c.Name.Equals(firstLang, StringComparison.OrdinalIgnoreCase));
                    return Task.FromResult<ProviderCultureResult?>(
                        new ProviderCultureResult(culture?.Name ?? defaultLanguage));
                })
            ]
        };

        return app.UseRequestLocalization(options);
    }
}
