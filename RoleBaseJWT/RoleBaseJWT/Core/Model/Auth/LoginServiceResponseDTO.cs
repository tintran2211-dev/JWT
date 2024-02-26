namespace RoleBaseJWT.Core.Model.Auth;

public class LoginServiceResponseDTO
{
    public string NewToken { get; set; }
    public UserInfoResult UserInfo { get; set; }
}