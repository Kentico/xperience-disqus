# Xperience Disqus Widget

Readme under construction

## Installation

1. Install this package in your .NET Core project
1. Create a [new application](https://disqus.com/api/applications/register/)
1. On the __Settings__ tab, set the __Callback URL__ to `https://<your MVC site>/disqus/auth`
1. Switch to the __Details__ tab and note the keys under the __OAuth Settings__ section
1. Open the [Disqus Admin](https://disqus.com/admin/)
1. If you see "What would you like to do with Disqus?" select "I want to install Disqus on my site"
1. Create a new site (or select an existing one) and note the __Shortname__ on the __Settings > General tab__
1. In your MVC project's startup code, initialize the Disqus integration: `services.AddDisqus();`
1. In your MVC project's `appsettings.json`, add the following section:

```json
"Disqus": {
    "Site": "<Shortname from step 5>",
    "ApiKey": "<API key from step 4>",
    "ApiSecret": "<API secret from step 4>",
    "AuthenticationRedirect": "https://<your MVC site>/disqus/auth"
}
```
## Requirements

The integration is currently built on __Kentico.Xperience.AspNetCore.WebApp__ 13.0.32

---

The default layout requires Bootstrap and jQuery. The following resources should be added to your main `_Layout.cshtml`:

```html
   <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-F3w7mX95PdgyTmZZMECAngseQB83DfGTowi0iMjiWaeVhAn4FJkqJByhZMI3AhiU" crossorigin="anonymous">
   <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js" integrity="sha384-KJ3o2DKtIkvYIK3UENzmM7KCkRr/rE9/Qpg6aAZGJwFDMVNA/GpGFF93hXpG5KkN" crossorigin="anonymous"></script>
   <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>
```

---

Your MVC project's routing must contain a "catch-all" route:

```cs
endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}",
    defaults: new {
        controller = "Home",
        action = "Index"
    }
);
```

Or, you can register a custom route for Disqus:

```cs
endpoints.MapControllerRoute(
    name: "Disqus",
    pattern: "disqus/{action}",
    defaults: new {
        controller = "Disqus"
    }
);
```