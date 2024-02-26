namespace RoleBaseJWT.Core.Model.Log;

public class GetLogDTO
{
    public DateTime CreateAt { get; set;} = DateTime.Now;
    
    public string? UserName { get; set; } 
    
    public string? Description { get; set; }    
}