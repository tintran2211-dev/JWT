using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleBaseJWT.Core.Constants;
using RoleBaseJWT.Core.IServices;
using RoleBaseJWT.Core.Model.Auth;

namespace RoleBaseJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        //Route => Seed roles to DB
        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            var seedResult = await _authService.SeedRolesAsync();
            return StatusCode(seedResult.StatusCode, seedResult.Message);
        }
        
        //Route => Register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            var registerResult = await _authService.RegisterAsync(registerDto);
            return StatusCode(registerResult.StatusCode, registerResult.Message);
        }
        
        //Route => Login
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginServiceResponseDTO>> Login([FromBody] LoginDTO loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);
            if (loginResult is null)
            {
                return Unauthorized("You credentials are invalid. Please contact to an Admin");
            }

            return Ok(loginResult);
        }
        
        //Route => Cập nhật vai trò người dùng
        //Chủ sở hữu có thể thay đổi mọi thứ
        //Quản trị viên chỉ có thể thay đổi người dùng thành người quản lý hoặc đảo ngược
        //Vai trò Manger và user không có quyền truy cập vào vao route nay
        [HttpPost]
        [Route("update-role")]
        [Authorize(Roles = StaticUserRoles.OwnerAdmin)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDTO updateRoleDto)
        {
            var updateRoleResult = await _authService.UpdateRolesAsync(User, updateRoleDto);
            if (updateRoleResult.IsSucceed)
            {
                return Ok(updateRoleResult.Message);
            }
            else
            {
                return StatusCode(updateRoleResult.StatusCode, updateRoleResult.Message);
            }
        }
        
        //Route => Lấy dữ liệu của người dùng từ JWT
        [HttpPost]
        [Route("me")]
        public async Task<ActionResult<LoginServiceResponseDTO>> Me([FromBody] MeDTO token)
        {
            try
            {
                var me = await _authService.MeAsync(token);
                if (me is not null)
                {
                    return Ok(me);
                }
                else
                {
                    return Unauthorized("Invalid Token");
                }
            }
            catch (Exception)
            {
                return Unauthorized("Invalid Token");
            }
        }
        
        //Route => Danh sách người dùng
        [HttpGet]
        [Route("users")]
        public async Task<ActionResult<IEnumerable<UserInfoResult>>> GetUsersList()
        {
            var usersList = await _authService.GetUserListAsync();
            return Ok(usersList);
        }
        
        //Route => Lay danh sach nguoi dung theo username
        [HttpGet]
        [Route("users/{userName}")]
        public async Task<ActionResult<UserInfoResult>> GetUsersByUserName([FromRoute]string userName)
        {
            var user = await _authService.GetUserDetailsByUserName(userName);
            if (user is not null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("UserName not found");
            }
        }
        
        //Route => Get danh sách tất cả tên người dùng để gửi tin nhắn
        [HttpGet]
        [Route("usernames")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNamesList()
        {
            var userNames = await _authService.GetUsernameListAsync();
            return Ok(userNames);
        }
    }
}
