namespace boya_usta_web.Services
{
    // Kimlik doğrulama servis arayüzü
    public interface IAuthService
    {
        Task<(bool Succeeded, string? ErrorMessage, bool IsLockedOut)> LoginAsync(string emailOrUsername, string password, bool rememberMe, string? ipAddress);
        Task LogoutAsync();
    }
}
