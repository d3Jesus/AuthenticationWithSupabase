# Authentication With Supabase

This example shows how to implement authentication in a .NET8 application with other providers like Google through Supabase.

In this example [Supabase](https://supabase.com/) was used for authentication. Before starting, create an account on the platform in order to access the Dashboard.

## Getting Started

Run this command in your terminal/command prompt:

```
dotnet add package Supabase --version 1.1.1
```

or

run this command in Package Manager:

```
NuGet\Install-Package Supabase -Version 1.1.1
```

or

add the following to your project:

```
<PackageReference Include="Supabase" Version="1.1.1" />
```

## Set the Supabase key and url

You can get your project key and url from the Supabase dashboard

```
{
  "Supabase": {
    "KEY": "",
    "URL": ""
  }
}
```

## Register Authentication and Authorization Required Services

```
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "auth.session";
    options.Cookie.MaxAge = TimeSpan.FromHours(1);
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth.cookie";
        options.LoginPath = "/login";
        options.Cookie.MaxAge = TimeSpan.FromHours(1);
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
```

## Custom endpoint for Social Authentication

This is a custom endpoint responsible for authentication through social media.

```
app.MapSocialIdentityEndpoints();
```

## Supabase Client Initialization

```
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        builder.Configuration["Supabase:URL"]!,
        builder.Configuration["Supabase:KEY"],
        new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = true
        }));
```

## Add the CascadingAuthenticationState Component

```
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        ...
    </Router>
</CascadingAuthenticationState>
```

## Custom MapSocialIdentityEndpoints

1. Set all providers that your app will use

```
Provider externalProvider = provider switch
{
    "Google" => Provider.Google,
    // set other providers..
    _ => Provider.Google
};
```

2. Create the options object. Make sure to set the `FlowType` to `OAuthFlowType.PKCE`.<br />
   The `RedirectTo` property should point to your app endpoint where you will complete the user authentication. The implementation is in the `AuthController.cs` file.

```
var options = new SignInOptions
{
    FlowType = OAuthFlowType.PKCE,
    RedirectTo = $"{context.Request.Scheme}://{context.Request.Host}/auth/callback"
};
```

3. Retrieve the `ProviderAuthState` and store it in a variable, for example `signInUrl`. Make sure that the `Uri` is not null.

```
var signInUrl = await supabase.Auth.SignIn(externalProvider, options);

if (signInUrl?.Uri is null)
{
    return Results.BadRequest("Invalid Uri");
}
```

4. If `signInUrl` is not null, store the value of `signInUrl.PKCEVerifier` and redirect the user to the Uri supplied by signInUrl.
   <br />You can read more about `PKCE` [here](https://oauth.net/2/pkce/).

```
context.Session.SetString(SessionCode.CodeVerifier, signInUrl.PKCEVerifier);

return Results.Redirect(signInUrl.Uri.AbsoluteUri);
```

## Supabase Documentation

You can find more about it in the [documentation](https://supabase.com/docs/reference/csharp/introduction).

## Author

- [Yuran de Jesus](https://github.com/d3Jesus)
