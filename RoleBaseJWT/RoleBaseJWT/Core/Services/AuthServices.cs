using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoleBaseJWT.Core.Constants;
using RoleBaseJWT.Core.Entities;
using RoleBaseJWT.Core.IServices;
using RoleBaseJWT.Core.Model.Auth;
using RoleBaseJWT.Core.Model.General;

namespace RoleBaseJWT.Core.Services;

public class AuthServices : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogServices _logServices;
    private readonly IConfiguration _configuration;

    public AuthServices(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogServices logServices, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logServices = logServices;
        _configuration = configuration;
    }

    public async Task<GeneralServiceResponseDTO> SeedRolesAsync()
    {
        bool isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);
        bool isAdminRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
        bool isManagerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.MANAGER);
        bool isUserRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
        if (isOwnerRoleExist && isAdminRoleExist && isManagerRoleExist && isUserRoleExist)
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = true,
                StatusCode = 200,
                Message = "Roles seeding is already done"
            };
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.MANAGER));
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
        
        return new GeneralServiceResponseDTO()
        {
            IsSucceed = true,
            StatusCode = 201,
            Message = "Roles seeding done successfully"
        };
    }

    public async Task<GeneralServiceResponseDTO> RegisterAsync(RegisterDTO registerDto)
    {
        //Kiểm tra User có tồn tại không
        var isExistUser = await _userManager.FindByNameAsync(registerDto.UserName);
        if(isExistUser is not null)
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 409,
                Message = "User already exist"
            };
        //Tạo mới user với các thông tin cần thiết
        User newUser = new User()
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Address = registerDto.Address,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        
        var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);
        //Check nếu tạo mới user lỗi
        if (!createUserResult.Succeeded)
        {
            var errorString = "User Creation failed because: ";
            foreach (var error in createUserResult.Errors)
            {
                errorString += " # " + error.Description + "; ";
            }
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 400,
                Message = errorString
            };
        }
        
        // Tạo các user có Role mặc định là USER
        await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
        await _logServices.SaveNewLog(newUser.UserName, "Registered to Website");

        return new GeneralServiceResponseDTO()
        {
            IsSucceed = true,
            StatusCode = 201,
            Message = "User created successfully"
        };
    }

    public async Task<LoginServiceResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        //Tìm user voi username và kiểm tra user đó có tồn tại.
        var user = await _userManager.FindByNameAsync(loginDto.UserName);
        if (user is null)
            return null;
        
        //Kiểm tra password
        var isPassword = await _userManager.CheckPasswordAsync(user ,loginDto.Password);
        if(!isPassword)
            return null;
        
        //Trả Token và thông tin user cho Frontend
        var newToken =await GenerateJwtTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = GenerateUserInfoObject(user, roles);
        await _logServices.SaveNewLog(user.UserName, "New Login");
        
        //Return phản hồi đăng nhập trả về Token mới và thông tin User
        return new LoginServiceResponseDTO()
        {
            NewToken = newToken,
            UserInfo = userInfo
        };

    }

    public async Task<GeneralServiceResponseDTO> UpdateRolesAsync(ClaimsPrincipal User, UpdateRoleDTO updateRoleDto)
    {
        //Kiểm tra username
        var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);
        if (user is null)
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 404,
                Message = "Invalid username"
            };
        
        var userRoles = await _userManager.GetRolesAsync(user);
        //Chỉ có role OWNER và ADMIN có thể cập nhật role
        if (User.IsInRole(StaticUserRoles.ADMIN))
        {
            //User là ADMIN
            if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.MANAGER)
            {
                if(userRoles.Any(x =>x.Equals(StaticUserRoles.OWNER) || x.Equals(StaticUserRoles.ADMIN)))
                {
                    return new GeneralServiceResponseDTO()
                    {
                        IsSucceed = false,
                        StatusCode = 403,
                        Message = "You are not allowed to change role of this user"
                    };
                }
                else
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                    await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                    await _logServices.SaveNewLog(user.UserName, "User Roles Updated");
                    return new GeneralServiceResponseDTO()
                    {
                        IsSucceed = true,
                        StatusCode = 200,
                        Message = "User Roles Updated"
                    };
                }
            }
            else return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 403,
                Message = "You are not allowed to change role of this user"
            };
        }
        else
        {
            //user là OWNER
            if (userRoles.Any(x => x.Equals(StaticUserRoles.OWNER)))
            {
                return new GeneralServiceResponseDTO()
                {
                    IsSucceed = false,
                    StatusCode = 403,
                    Message = "You are not allowed to change role of this user"
                };
            }
            else
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                await _logServices.SaveNewLog(user.UserName, "User Roles Updated");
                return new GeneralServiceResponseDTO()
                {
                    IsSucceed = true,
                    StatusCode = 200,
                    Message = "User Roles Updated"
                };
            }
        }
    }

    //Lam moi token
    public async Task<LoginServiceResponseDTO?> MeAsync(MeDTO meDto)
    {
        ClaimsPrincipal handle = new JwtSecurityTokenHandler().ValidateToken(meDto.Token,
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            }, out SecurityToken securityToken);

        string decodedUserName = handle.Claims.First(x => x.Type == ClaimTypes.Name).Value;
        if(decodedUserName is null)
            return null;
        
        var user = await _userManager.FindByNameAsync(decodedUserName);
        if (user is null)
            return null;
        
        //Trả Token và thông tin user cho Frontend
        var newToken = await GenerateJwtTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = GenerateUserInfoObject(user, roles);
        await _logServices.SaveNewLog(user.UserName, "New Token Generated");
        
        //Return phản hồi đăng nhập trả về Token mới và thông tin User
        return new LoginServiceResponseDTO()
        {
            NewToken = newToken,
            UserInfo = userInfo
        };
    }

    public async Task<IEnumerable<UserInfoResult>> GetUserListAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        
        List<UserInfoResult> userInfoResults = new List<UserInfoResult>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);
            userInfoResults.Add(userInfo);
        }
        return userInfoResults;
    }

    public async Task<UserInfoResult?> GetUserDetailsByUserName(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        
        if (user is null)
            return null;
        
        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = GenerateUserInfoObject(user, roles);
        return userInfo;
    }

    public async Task<IEnumerable<string>> GetUsernameListAsync()
    {
        var username = await _userManager.Users
            .Select(x => x.UserName)
            .ToListAsync();
        return username;
    }
    
    //Hàm GeneratejwtToken(Khởi tạo jwt token)
    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var authClaim = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };
        foreach (var userRole in userRoles)
        {
            authClaim.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
        var signingCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

        var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                notBefore: DateTime.Now,
                claims: authClaim,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: signingCredentials
            );

        string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
        return token;
    }
    
    //Hàm GenerateUserInfoResult mapping User với UserInfo, để map UserInfo với User có thể dùng AutoMapper
    private UserInfoResult GenerateUserInfoObject(User user, IEnumerable<string> Roles)
    {
        return new UserInfoResult()
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatAt = user.CreatedDate,
            Roles = Roles
        };
    }
}