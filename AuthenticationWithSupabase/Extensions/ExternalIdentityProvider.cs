using AuthenticationWithSupabase.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Supabase.Gotrue;
using System.Security.Claims;
using static Supabase.Gotrue.Constants;

namespace AuthenticationWithSupabase.Extensions;

public static class ExternalIdentityProvider
{
    public static IEndpointConventionBuilder MapSocialIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/auth").AllowAnonymous();

        accountGroup.MapPost("/external-login", async (
            HttpContext context,
            [FromServices] Supabase.Client supabase,
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            Provider externalProvider = provider switch
            {
                "Google" => Provider.Google,
                "Facebook" => Provider.Facebook,
                "Github" => Provider.Github,
                _ => Provider.Google
            };

            var options = new SignInOptions
            {
                FlowType = OAuthFlowType.PKCE,
                RedirectTo = "http://localhost:5024/auth/callback"
            };

            var signInUrl = await supabase.Auth.SignIn(externalProvider, options);

            if (signInUrl?.Uri is null)
            {
                return Results.BadRequest("Invalid Uri");
            }

            context.Session.SetString(SessionCode.CodeVerifier, signInUrl.PKCEVerifier);

            return Results.Redirect(signInUrl.Uri.AbsoluteUri);
        });

        return accountGroup;
    }
}
