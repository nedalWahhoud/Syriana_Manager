using Syriana_Manager.Components.ProductsF;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    public const string Permission = "Permission";
    public CustomAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string token = null!;
            ClaimsPrincipal user;
            string localToken = await LocalstorageGet("authToken");
            string sessionToken = await SessionStorageGet("authToken");

            if (localToken == null && sessionToken == null)
            {
                // User is not logged
                user = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(user);
            }
            else
            {
                token = localToken ?? sessionToken;
            }

            var identity = GetIdentity(token);
            NotifyUserAuthentication(token);

            user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    public void NotifyUserAuthentication(string token)
    {
        var identity = GetIdentity(token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }
    public async Task NotifyUserLogout()
    {
        await LocalstorageRemove("authToken");
       
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
    public async Task LocalstorageSet(string key, string value)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }
    private async Task LocalstorageRemove(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
    public async Task<string> LocalstorageGet(string key)
    {
        string value = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return value;
    }
    public async Task SessionStorageSet(string key, string value)
    {
        await _js.InvokeVoidAsync("sessionStorage.setItem", key, value);
    }

    public async Task<string> SessionStorageGet(string key)
    {
        return await _js.InvokeAsync<string>("sessionStorage.getItem", key);
    }

    /*private async Task SessionStorageRemove(string key)
    {
        await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
    }*/
    public ClaimsIdentity GetIdentity(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // get claims from jwtToken
        var wantedClaims = new[] {
             ClaimTypes.NameIdentifier,
             ClaimTypes.Name,
             ClaimTypes.Email,
             ClaimTypes.Role,
             ClaimTypes.DateOfBirth,
             Permission
            };

        var filteredClaims = jwtToken.Claims
            .Where(c => wantedClaims.Contains(c.Type))
            .Select(c => new Claim(c.Type, c.Value))
            .ToList();

        var identity = new ClaimsIdentity(filteredClaims, "claim");

        return identity;
    }
}
