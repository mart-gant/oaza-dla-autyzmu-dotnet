using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OazaDlaAutyzmu.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserInfoKey = "user_info";

    public event Action? OnAuthStateChanged;
    public event Action? OnSensorySettingsChanged;

    public bool IsAuthenticated { get; private set; }
    public UserProfile? CurrentUser { get; private set; }

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        InitializeAuth();
    }

    private async void InitializeAuth()
    {
        try
        {
            var token = await GetSecureValueAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                IsAuthenticated = true;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var userInfoJson = Preferences.Default.Get(UserInfoKey, string.Empty);
                if (!string.IsNullOrEmpty(userInfoJson))
                {
                    CurrentUser = JsonSerializer.Deserialize<UserProfile>(userInfoJson);
                }
            }
        }
        catch
        {
            // Fallback if secure storage is unavailable
        }
    }

    #region Authentication API

    public async Task<ApiResponse<bool>> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", new
            {
                email,
                password,
                firstName,
                lastName
            });

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResult(true);
            }

            var errorData = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ApiResponse<bool>.ErrorResult(errorData?.Message ?? "Rejestracja nie powiodła się.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd połączenia: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", new
            {
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorData = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return ApiResponse<bool>.ErrorResult(errorData?.Message ?? "Nieprawidłowy email lub hasło.");
            }

            var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                return ApiResponse<bool>.ErrorResult("Błąd serwera: brak tokenu w odpowiedzi.");
            }

            await SetSecureValueAsync(TokenKey, tokenData.AccessToken);
            if (!string.IsNullOrEmpty(tokenData.RefreshToken))
            {
                await SetSecureValueAsync(RefreshTokenKey, tokenData.RefreshToken);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
            IsAuthenticated = true;

            // Fetch current user details
            var profileResponse = await GetUserProfileAsync();
            if (profileResponse.IsSuccess && profileResponse.Data != null)
            {
                CurrentUser = profileResponse.Data;
                Preferences.Default.Set(UserInfoKey, JsonSerializer.Serialize(CurrentUser));
            }

            OnAuthStateChanged?.Invoke();
            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd połączenia: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ExternalLoginAsync(string provider)
    {
        try
        {
            var backendUrl = GetBackendUrl();
            if (string.IsNullOrEmpty(backendUrl))
            {
                return ApiResponse<bool>.ErrorResult("Nie skonfigurowano adresu URL serwera API.");
            }

            var callbackUrl = "oazadlaautyzmu://callback";
            var authUrl = $"{backendUrl.TrimEnd('/')}/api/v1/auth/external-login?provider={provider}&redirectUri={Uri.EscapeDataString(callbackUrl)}";
            
            var authResult = await Microsoft.Maui.Authentication.WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri(callbackUrl));

            var accessToken = authResult.Properties.TryGetValue("accessToken", out var at) ? at : null;
            var refreshToken = authResult.Properties.TryGetValue("refreshToken", out var rt) ? rt : null;

            if (string.IsNullOrEmpty(accessToken))
            {
                return ApiResponse<bool>.ErrorResult("Błąd uwierzytelniania: brak tokenu dostępu.");
            }

            await SetSecureValueAsync(TokenKey, accessToken);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await SetSecureValueAsync(RefreshTokenKey, refreshToken);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            IsAuthenticated = true;

            // Fetch current user details
            var profileResponse = await GetUserProfileAsync();
            if (profileResponse.IsSuccess && profileResponse.Data != null)
            {
                CurrentUser = profileResponse.Data;
                Preferences.Default.Set(UserInfoKey, JsonSerializer.Serialize(CurrentUser));
            }

            OnAuthStateChanged?.Invoke();
            return ApiResponse<bool>.SuccessResult(true);
        }
        catch (TaskCanceledException)
        {
            return ApiResponse<bool>.ErrorResult("Uwierzytelnianie zostało anulowane przez użytkownika.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd logowania zewnętrznego: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, string.Empty);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, string.Empty);
        }
        catch
        {
            // SecureStorage fallback
        }
        
        Preferences.Default.Remove(UserInfoKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        IsAuthenticated = false;
        CurrentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    private async Task<ApiResponse<UserProfile>> GetUserProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserProfile>();
                return ApiResponse<UserProfile>.SuccessResult(user!);
            }
            return ApiResponse<UserProfile>.ErrorResult("Nie udało się pobrać danych profilu.");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserProfile>.ErrorResult(ex.Message);
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await GetSecureValueAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/refresh", new { refreshToken });
            if (response.IsSuccessStatusCode)
            {
                var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokenData != null && !string.IsNullOrEmpty(tokenData.AccessToken))
                {
                    await SetSecureValueAsync(TokenKey, tokenData.AccessToken);
                    if (!string.IsNullOrEmpty(tokenData.RefreshToken))
                    {
                        await SetSecureValueAsync(RefreshTokenKey, tokenData.RefreshToken);
                    }
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
                    return true;
                }
            }
        }
        catch
        {
            // Handle refresh token failure
        }
        await LogoutAsync();
        return false;
    }

    #endregion

    #region Facilities API

    public async Task<ApiResponse<FacilityListResponse>> GetFacilitiesAsync(string? search = null, string? city = null, string? type = null, int page = 1, int pageSize = 12)
    {
        try
        {
            var query = $"?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(city)) query += $"&city={Uri.EscapeDataString(city)}";
            if (!string.IsNullOrEmpty(type)) query += $"&type={type}";

            var response = await _httpClient.GetAsync($"api/v1/facilities{query}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<FacilityListResponse>();
                return ApiResponse<FacilityListResponse>.SuccessResult(data!);
            }
            return ApiResponse<FacilityListResponse>.ErrorResult("Nie udało się pobrać placówek.");
        }
        catch (Exception ex)
        {
            return ApiResponse<FacilityListResponse>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FacilityDetail>> GetFacilityDetailsAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/facilities/{id}");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<JsonElementWrapper<FacilityDetail>>();
                return ApiResponse<FacilityDetail>.SuccessResult(wrapper!.Data);
            }
            return ApiResponse<FacilityDetail>.ErrorResult("Nie znaleziono placówki.");
        }
        catch (Exception ex)
        {
            return ApiResponse<FacilityDetail>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ReviewItem>>> GetFacilityReviewsAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/facilities/{id}/reviews");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<JsonElementWrapper<List<ReviewItem>>>();
                return ApiResponse<List<ReviewItem>>.SuccessResult(wrapper!.Data);
            }
            return ApiResponse<List<ReviewItem>>.ErrorResult("Błąd pobierania recenzji.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ReviewItem>>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> CreateReviewAsync(int facilityId, int rating, string comment)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/reviews", new
            {
                facilityId,
                rating,
                comment
            });

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResult(true);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await RefreshTokenAsync();
                if (refreshed) return await CreateReviewAsync(facilityId, rating, comment);
                return ApiResponse<bool>.ErrorResult("Zaloguj się, aby dodać recenzję.");
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ApiResponse<bool>.ErrorResult(error?.Message ?? "Nie udało się dodać recenzji. Sprawdź, czy nie oceniałeś już tej placówki.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    #endregion

    #region Forum API

    public async Task<ApiResponse<List<ForumCategory>>> GetForumCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/forum/categories");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<JsonElementWrapper<List<ForumCategory>>>();
                return ApiResponse<List<ForumCategory>>.SuccessResult(wrapper!.Data);
            }
            return ApiResponse<List<ForumCategory>>.ErrorResult("Nie udało się pobrać kategorii forum.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ForumCategory>>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ForumTopic>>> GetCategoryTopicsAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/forum/categories/{categoryId}/topics");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<JsonElementWrapper<List<ForumTopic>>>();
                return ApiResponse<List<ForumTopic>>.SuccessResult(wrapper!.Data);
            }
            return ApiResponse<List<ForumTopic>>.ErrorResult("Nie udało się pobrać tematów.");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ForumTopic>>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TopicDetailsWrapper>> GetTopicDetailsAsync(int topicId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/forum/topics/{topicId}");
            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<JsonElementWrapper<TopicDetailsWrapper>>();
                return ApiResponse<TopicDetailsWrapper>.SuccessResult(wrapper!.Data);
            }
            return ApiResponse<TopicDetailsWrapper>.ErrorResult("Nie znaleziono tematu.");
        }
        catch (Exception ex)
        {
            return ApiResponse<TopicDetailsWrapper>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> CreateTopicAsync(int categoryId, string title, string content)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/forum/topics", new
            {
                categoryId,
                title,
                content
            });

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResult(true);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await RefreshTokenAsync();
                if (refreshed) return await CreateTopicAsync(categoryId, title, content);
                return ApiResponse<bool>.ErrorResult("Zaloguj się, aby założyć temat.");
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ApiResponse<bool>.ErrorResult(error?.Message ?? "Nie udało się utworzyć tematu.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> CreatePostAsync(int topicId, string content)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/forum/topics/{topicId}/posts", new
            {
                content
            });

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResult(true);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshed = await RefreshTokenAsync();
                if (refreshed) return await CreatePostAsync(topicId, content);
                return ApiResponse<bool>.ErrorResult("Zaloguj się, aby dodać odpowiedź.");
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return ApiResponse<bool>.ErrorResult(error?.Message ?? "Nie udało się dodać odpowiedzi.");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Błąd: {ex.Message}");
        }
    }

    #endregion

    #region Local Helpers & Settings

    private async Task<string> GetSecureValueAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key) ?? string.Empty;
        }
        catch
        {
            return Preferences.Default.Get(key, string.Empty);
        }
    }

    private async Task SetSecureValueAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch
        {
            Preferences.Default.Set(key, value);
        }
    }

    public double GetTextScale() => Preferences.Default.Get("SensoryTextScale", 1.0);
    public void SetTextScale(double scale)
    {
        Preferences.Default.Set("SensoryTextScale", scale);
        OnSensorySettingsChanged?.Invoke();
    }

    public bool GetHighContrast() => Preferences.Default.Get("SensoryHighContrast", false);
    public void SetHighContrast(bool enabled)
    {
        Preferences.Default.Set("SensoryHighContrast", enabled);
        OnSensorySettingsChanged?.Invoke();
    }

    public bool GetReducedMotion() => Preferences.Default.Get("SensoryReducedMotion", false);
    public void SetReducedMotion(bool enabled)
    {
        Preferences.Default.Set("SensoryReducedMotion", enabled);
        OnSensorySettingsChanged?.Invoke();
    }

    public bool GetCalmMode() => Preferences.Default.Get("SensoryCalmMode", false);
    public void SetCalmMode(bool enabled)
    {
        Preferences.Default.Set("SensoryCalmMode", enabled);
        OnSensorySettingsChanged?.Invoke();
    }

    public string GetBackendUrl() => Preferences.Default.Get("BackendUrl", _httpClient.BaseAddress?.ToString() ?? string.Empty);
    public void SetBackendUrl(string url)
    {
        Preferences.Default.Set("BackendUrl", url);
        _httpClient.BaseAddress = new Uri(url);
    }

    #endregion
}

#region Helper Models

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ApiResponse<T> SuccessResult(T data) => new() { IsSuccess = true, Data = data };
    public static ApiResponse<T> ErrorResult(string msg) => new() { IsSuccess = false, Message = msg };
}

public class TokenResponse
{
    public string TokenType { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class JsonElementWrapper<T>
{
    public T Data { get; set; } = default!;
}

public class UserProfile
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FacilityListResponse
{
    public List<FacilityItem> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class FacilityItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class FacilityDetail : FacilityItem
{
    public string PostalCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class ReviewItem
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ForumCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TopicCount { get; set; }
    public int PostCount { get; set; }
}

public class ForumTopic
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string AuthorName { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public DateTime LastPostAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ForumPost
{
    public int Id { get; set; }
    public int TopicId { get; set; }

    [JsonPropertyName("userId")]
    public int AuthorId { get; set; }

    [JsonPropertyName("userName")]
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? EditedAt { get; set; }
}

public class TopicDetailsWrapper
{
    public ForumTopic Topic { get; set; } = default!;
    public PaginatedList<ForumPost> Posts { get; set; } = default!;
}

public class PaginatedList<T>
{
    public List<T> Data { get; set; } = new();
}

#endregion
