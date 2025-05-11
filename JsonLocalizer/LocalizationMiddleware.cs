using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace JsonLocalizer;

internal class LocalizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var languageHeader = context.Request.Headers.AcceptLanguage.ToString();
        var primaryLanguage = languageHeader.Split(',').FirstOrDefault();
        var cultureKey = primaryLanguage?.Split(';').FirstOrDefault() ?? "en-US";
        if (!string.IsNullOrEmpty(cultureKey))
        {
            if (DoesCultureExist(cultureKey!))
            {
                var culture = new CultureInfo(cultureKey!);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
        await next(context);
    }
    private static bool DoesCultureExist(string cultureName)
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Any(culture => string
            .Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
    }
}
