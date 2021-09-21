# Xperience Disqus Widget

Readme under construction

## Installation

1. Install this package in your .NET Core project
1. Create a new application at https://disqus.com/api/applications/register/
1. On the __Settings__ tab, set the __Callback URL__ to `https://<your MVC site>/disqus/auth`
1. Switch to the __Details__ tab and note the keys under the __OAuth Settings__ section
1. In [Disqus Admin](https://disqus.com/admin/), locate or create a new site and note the __Shortname__ on the __General tab__
1. In your .NET Core project's `appsettings.json`, add the following section:

```json
"Disqus": {
    "Site": "<Shortname from step 5>",
    "ApiKey": "<API key from step 4>",
    "ApiSecret": "<API secret from step 4>",
    "AuthenticationRedirect": "https://<your MVC site>/disqus/auth"
}
```
