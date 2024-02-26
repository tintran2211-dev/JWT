using System.Security.Claims;
using RoleBaseJWT.Core.Model.Auth;
using RoleBaseJWT.Core.Model.General;

namespace RoleBaseJWT.Core.IServices;

public interface IAuthService
{
    Task<GeneralServiceResponseDTO> SeedRolesAsync();
    Task<GeneralServiceResponseDTO> RegisterAsync(RegisterDTO registerDto);
    Task<LoginServiceResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<GeneralServiceResponseDTO> UpdateRolesAsync(ClaimsPrincipal User, UpdateRoleDTO updateRoleDto);
    Task<LoginServiceResponseDTO?> MeAsync(MeDTO meDto);
    Task<IEnumerable<UserInfoResult>> GetUserListAsync();
    Task<UserInfoResult?> GetUserDetailsByUserName(string userName);
    Task<IEnumerable<string>> GetUsernameListAsync();
}