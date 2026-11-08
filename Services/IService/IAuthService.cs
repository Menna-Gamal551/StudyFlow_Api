using StudyFlow_Api.Data.DTOs;

namespace StudyFlow_Api.Services.IService
{
    public interface IAuthService
    {
        public Task<UserDTO?> RegisterAsync(RegisterRequestDTO registerRequestDTO);
        public Task<bool> IsUserNameExistAsync(string userName);
        public Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);
    }
}
