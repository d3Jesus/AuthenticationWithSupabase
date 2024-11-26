using AuthenticationWithSupabase.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticationWithSupabase.Controllers;

[Route("auth/callback")]
public class AuthController(Supabase.Client client, IHttpContextAccessor contextAccessor) : Controller
{
    public async Task<IActionResult> Callback()
    {
        // Provided by the provider
        var authcode = Request.Query["code"];
        if (string.IsNullOrEmpty(authcode))
        {
            return BadRequest();
        }

        string codeVerifier = contextAccessor.HttpContext.Session.GetString(SessionCode.CodeVerifier);
        if (string.IsNullOrEmpty(codeVerifier))
        {
            return BadRequest();
        }

        Supabase.Gotrue.Session session = await client.Auth.ExchangeCodeForSession(codeVerifier, authcode);
        if (session is null)
        {
            return LocalRedirect("/");
        }

        var claims = new List<Claim>()
        {
            new(ClaimTypes.Sid, session.User.Id),
            new(ClaimTypes.Email, session.User.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await contextAccessor.HttpContext.SignInAsync(principal);

        return LocalRedirect("/counter");
    }
}
