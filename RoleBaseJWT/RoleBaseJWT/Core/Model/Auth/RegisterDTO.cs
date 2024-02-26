using System.ComponentModel.DataAnnotations;

namespace RoleBaseJWT.Core.Model.Auth;

public class RegisterDTO
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; }
    
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
    
    public string Address { get; set; }
}