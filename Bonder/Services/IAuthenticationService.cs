using Bonder.Models;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Bonder.Services;

public interface IAuthenticationService
{
    Task<AuthResult> SignInWithEmailAsync(string email, string password);
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, string name);
    Task<AuthResult> SignInWithGoogleAsync();
    Task<AuthResult> SignInWithAppleAsync();
    Task SignOutAsync();
    Task<User> GetCurrentUserAsync();
    bool IsAuthenticated { get; }
}

public class FirebaseAuthService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;
    private const string FirebaseApiKey = "YOUR_FIREBASE_API_KEY"; // Replace with your key
    private User _currentUser;

    public bool IsAuthenticated => _currentUser != null;

    public FirebaseAuthService(IStorageService storageService)
    {
        _httpClient = new HttpClient();
        _storageService = storageService;
        _ = LoadCurrentUserAsync();
    }

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            var requestBody = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}",
                requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
                await SaveUserSession(result);
                return new AuthResult { Success = true, User = _currentUser };
            }

            var error = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, ErrorMessage = ParseFirebaseError(error) };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, string name)
    {
        try
        {
            var requestBody = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}",
                requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();

                // Create user profile
                _currentUser = new User
                {
                    Id = result.LocalId,
                    Email = email,
                    Name = name,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveUserSession(result);
                await _storageService.SaveUserProfileAsync(_currentUser);

                return new AuthResult { Success = true, User = _currentUser };
            }

            var error = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, ErrorMessage = ParseFirebaseError(error) };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AuthResult> SignInWithGoogleAsync()
    {
        // Implement Google Sign-In using platform-specific code
        // For MAUI, you'll need to use WebAuthenticator
        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri("YOUR_GOOGLE_OAUTH_URL"),
                    CallbackUrl = new Uri("com.bonder.app://callback")
                });

            // Process the OAuth token
            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<AuthResult> SignInWithAppleAsync()
    {
        // Implement Apple Sign-In using platform-specific code
        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri("YOUR_APPLE_OAUTH_URL"),
                    CallbackUrl = new Uri("com.bonder.app://callback")
                });

            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task SignOutAsync()
    {
        _currentUser = null;
        Preferences.Default.Remove("auth_token");
        Preferences.Default.Remove("refresh_token");
        Preferences.Default.Remove("user_id");
        await Task.CompletedTask;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        await LoadCurrentUserAsync();
        return _currentUser;
    }

    private async Task LoadCurrentUserAsync()
    {
        var userId = Preferences.Default.Get("user_id", string.Empty);
        if (!string.IsNullOrEmpty(userId))
        {
            _currentUser = await _storageService.LoadUserProfileAsync();
        }
    }

    private async Task SaveUserSession(FirebaseAuthResponse response)
    {
        Preferences.Default.Set("auth_token", response.IdToken);
        Preferences.Default.Set("refresh_token", response.RefreshToken);
        Preferences.Default.Set("user_id", response.LocalId);
        Preferences.Default.Set("user_email", response.Email);

        if (_currentUser == null)
        {
            _currentUser = new User
            {
                Id = response.LocalId,
                Email = response.Email
            };
        }

        await Task.CompletedTask;
    }

    private string ParseFirebaseError(string error)
    {
        if (error.Contains("EMAIL_NOT_FOUND"))
            return "Email not found";
        if (error.Contains("INVALID_PASSWORD"))
            return "Invalid password";
        if (error.Contains("EMAIL_EXISTS"))
            return "Email already exists";
        if (error.Contains("WEAK_PASSWORD"))
            return "Password should be at least 6 characters";

        return "Authentication failed";
    }

    private class FirebaseAuthResponse
    {
        public string IdToken { get; set; }
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public string LocalId { get; set; }
        public int ExpiresIn { get; set; }
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public User User { get; set; }
    public string ErrorMessage { get; set; }
}