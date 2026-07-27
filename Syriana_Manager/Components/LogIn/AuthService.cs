using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Syriana_Manager.Components.LogIn
{
    public class AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
    {
        private readonly HttpClient _http = http;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        public async Task<ValidationResult> LoginAsync(LoginModel loginModel)
        {
            try
            {
                HttpResponseMessage response = await _http!.PostAsJsonAsync("api/Users/login", loginModel);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    return new ValidationResult
                    {
                        Result = false,
                        Message = $"Status: {(int)response.StatusCode} {response.StatusCode}\n{error}"
                    };
                }
                // get result
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                    return new ValidationResult { Result = false, Message = "Login failed. No token received." };

                // check admin role 
                var claimsIdentity = (_authStateProvider as CustomAuthStateProvider)?.GetIdentity(result.Token);
                if (!claimsIdentity!.HasClaim(ClaimTypes.Role, "admin"))
                    return new ValidationResult { Result = false, Message = "Sie haben keine Admin Rechte" };

                // set the authorization header
                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result.Token);
                if (loginModel.RememberMe)
                    (_authStateProvider as CustomAuthStateProvider)?.LocalstorageSet("authToken", result.Token);
                else
                    (_authStateProvider as CustomAuthStateProvider)?.SessionStorageSet("authToken", result.Token);

                return new ValidationResult { Result = true, Message = "Login successful." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"An error occurred during login: {ex.Message}" };
            }
        }
        public async Task Logout()
        {
            try
            {
                if (_authStateProvider is CustomAuthStateProvider customAuthStateProvider)
                {
                    await customAuthStateProvider.NotifyUserLogout();
                }
                _http!.DefaultRequestHeaders.Authorization = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during logout: {ex.Message}");
            }
        }
        public async Task<LoginModel> GetItemsUsersAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _http!.GetAsync($"api/Users/getUserById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var result = await response.Content.ReadFromJsonAsync<LoginModel>();
                return result ?? null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<ValidationResult> UpdateProfileAsync(UpdateProfile updateProfile)
        {
            try
            {
                var response = await _http!.PutAsJsonAsync("api/Users/update", updateProfile);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }

                // get result token
                var result1 = await response.Content.ReadFromJsonAsync<LoginModel>();

                if (result1 == null || string.IsNullOrEmpty(result1.Token))
                    return new ValidationResult { Result = false, Message = "Token Error" };

                return new ValidationResult { Result = true, Message = "erfolgreich Userdata geupdatet" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // google login
        public ValidationResult GoogleLogin(LoginModel loginModel)
        {
            try
            {
                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(loginModel.Token);

                // Save token to localStorage
                if (loginModel.RememberMe)
                    (_authStateProvider as CustomAuthStateProvider)?.LocalstorageSet("authToken", loginModel.Token);
                else
                    (_authStateProvider as CustomAuthStateProvider)?.SessionStorageSet("authToken", loginModel.Token);

                return new ValidationResult { Result = true, Message = "erfolgreich eingeloggt" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // users
        public GetItems<Users> GetItemsUsers { get; set; } = new();
        public List<Users> DownloadedUsers { get; set; } = [];
        public async Task<ValidationResult> GetAllUsers()
        {
            if (GetItemsUsers.AllItemsLoaded)
            {
                return new ValidationResult() { Result = true, Message = string.Empty };
            }

            try
            {
                HttpResponseMessage response = await _http.GetAsync($"api/Users/getAllUsers?CurrentPage={GetItemsUsers.CurrentPage}&PageSize={GetItemsUsers.PageSize}&AllItemsLoaded={GetItemsUsers.AllItemsLoaded}");
                var result = await response.Content.ReadFromJsonAsync<GetItems<Users>>();
                if (result == null)
                    return new ValidationResult() { Result = false, Message = "" };
                else
                {
                    GetItemsUsers.CurrentPage = result.CurrentPage;
                    GetItemsUsers.AllItemsLoaded = result.AllItemsLoaded;

                    AddToLocal(result.Items);
                    return new ValidationResult() { Result = true, Message = "" };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult() { Result = false, Message = ex.Message };
            }
        }
        // local
        public async Task<Users> GetUser()
        {
            Users userModel = new();

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    userModel.Id = userId;
                }
                userModel.SignupProvider = user.FindFirst(c => c.Type == "SignupProvider")?.Value!;
            }

            return userModel;
        }
        public void AddToLocal(List<Users> users)
        {
            if (users.Count > 0 && DownloadedUsers.Count == 0)
            {
                DownloadedUsers.AddRange(users);
                return;
            }
            foreach (var user in users)
            {
                if (!DownloadedUsers.Any(p => p.Id == user.Id))
                {
                    DownloadedUsers.Add(user);
                }
            }
        }
        public void AddToLocal(Users user, int index)
        {
            if (!DownloadedUsers.Any(p => p.Id == user.Id))
            {
                DownloadedUsers.Insert(index, user);
            }
        }
    }
}
