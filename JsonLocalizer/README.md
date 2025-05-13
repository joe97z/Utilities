# Localizer.Json

Localizer.Json is a lightweight library for enabling JSON-based localization in ASP.NET Core applications. It provides extension methods to configure localization services and middleware, allowing you to manage translations using JSON files.

## Prerequisites

- .NET SDK 9.0
- ASP.NET Core application

## Installation

1. Add the `Localizer.Json` namespace to your project by including the provided `JsonLocalizationExtension.cs` file in your solution.

2. Ensure the following NuGet packages are installed in your project:
   - `Microsoft.Extensions.Localization`
   - `Microsoft.AspNetCore.Localization`

   You can install them via the Package Manager Console:
   ```bash
   Install-Package Microsoft.Extensions.Localization
   Install-Package Microsoft.AspNetCore.Localization
   ```

## Setup

### 1. Create a Localization Folder

Create a folder named `localizations` in the root of your project. This folder will store your localization files.

### 2. Add Localization JSON Files

Inside the `localizations` folder, create JSON files named in the format `localization.{lang}.json`, where `{lang}` is the culture code (e.g., `en`, `fr`, `es`). Example structure:

```
localizations/
├── localization.en.json
├── localization.fr.json
├── localization.es.json
```

Example content for `localization.en.json`:

```json
{
  "Greeting": "Hello, World!",
  "Welcome": "Welcome to our application!"
}
```

Example content for `localization.fr.json`:

```json
{
  "Greeting": "Bonjour le monde !",
  "Welcome": "Bienvenue dans notre application !"
}
```

### 3. Configure Localization in Your Application

In your `Program.cs` or `Startup.cs`, configure the JSON-based localization by adding the following code:

```csharp
using Localizer.Json;
using System.Globalization;

// Create a new web application builder
var builder = WebApplication.CreateBuilder(args);

// Add JSON localization services
builder.AddJsonLocalization();

// Add controllers or other services as needed
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure supported cultures and default language
var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("en"),
    new CultureInfo("fr"),
    new CultureInfo("es")
};

// Use JSON localization middleware
app.UseJsonLocalization(supportedCultures, "en");

// Add other middleware as needed
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 4. Inject and Use IStringLocalizer

In your controllers, services, or views, inject `IStringLocalizer` to access localized strings:

```csharp
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IStringLocalizer<HomeController> _localizer;

    public HomeController(IStringLocalizer<HomeController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        ViewData["Greeting"] = _localizer["Greeting"];
        ViewData["Welcome"] = _localizer["Welcome"];
        return View();
    }
}
```

In Razor views, you can use `IStringLocalizer` or inject it via `@inject`:

```cshtml
@inject IStringLocalizer<SharedResource> Localizer

<h1>@Localizer["Greeting"]</h1>
<p>@Localizer["Welcome"]</p>
```

### 5. Test Localization

To test different languages, include the `Accept-Language` header in your HTTP requests. For example:

- For English: `Accept-Language: en`
- For French: `Accept-Language: fr`
- For Spanish: `Accept-Language: es`

You can use tools like Postman or modify browser language settings to simulate different `Accept-Language` headers.

## How It Works

- **Service Registration**: The `AddJsonLocalization` extension method registers the necessary localization services, including a custom `JsonStringLocalizerFactory` to load translations from JSON files.
- **Middleware Configuration**: The `UseJsonLocalization` extension method sets up request localization with a custom `CustomRequestCultureProvider` that determines the culture based on the `Accept-Language` header.
- **JSON Files**: Translations are stored in `localization.{lang}.json` files in the `localizations` folder. The `JsonStringLocalizerFactory` reads these files to provide localized strings.

## Notes

- Ensure that the `localizations` folder is in the root of your project and contains valid JSON files.
- The `defaultLanguage` parameter in `UseJsonLocalization` is used as a fallback if the requested culture is not supported.
- The library assumes that the JSON files are named strictly as `localization.{lang}.json` (e.g., `localization.en.json`).

## Troubleshooting

- **Missing Translations**: Verify that the JSON files exist in the `localizations` folder and are correctly formatted.
- **Culture Not Applied**: Ensure the `Accept-Language` header is correctly set in the request or that the default language is properly configured.
- **Dependency Injection Issues**: Confirm that `IStringLocalizer` is injected correctly in your controllers or views.

## License

This project is licensed under the MIT License.

## Changelog
### Version 1.1.0
- **Fixed**: Corrected a critical typo in the JsonStringLocalizer class hat could cause incorrect loading of localization files.
