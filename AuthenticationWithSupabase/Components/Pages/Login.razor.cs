using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AuthenticationWithSupabase.Components.Pages.Auth;

public partial class Login
{

    [Parameter]
    public EventCallback<string> OnFormChange { get; set; }
    private async Task ToggleToRegisterForm()
    {
        await OnFormChange.InvokeAsync("active");
    }

    private string message = string.Empty;
    [Inject]
    public IHttpContextAccessor Accessor { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel LoginInput { get; set; } = new();

    [Inject]
    public NavigationManager Navigation { get; set; }

    [Inject]
    public Supabase.Client Supabase { get; set; }

    public async Task LoginUser()
    {
        Supabase.Gotrue.Session session = await Supabase.Auth.SignIn(LoginInput.Email, LoginInput.Password);
        if (session is not null)
        {
            var containsFullName = session.User.UserMetadata.ContainsKey("full_name");

            var claims = new List<Claim>()
            {
                new(ClaimTypes.Sid, session.User.Id),
                new("full_name", containsFullName ? session.User.UserMetadata["full_name"].ToString() : "No name")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await Accessor.HttpContext!.SignInAsync(principal);

            Navigation.NavigateTo("/counter", true);
        }
        else
        {
            message = "";
        }
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "O email fornecido é inválido.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
