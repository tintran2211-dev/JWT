using System.Security.Claims;
using RoleBaseJWT.Core.Model.Log;

namespace RoleBaseJWT.Core.IServices;

public interface ILogServices
{
    Task SaveNewLog(string UserName, string Description);
    Task<IEnumerable<GetLogDTO>> GetLogsAsync();
    Task<IEnumerable<GetLogDTO>> GetMyLogsAsync(ClaimsPrincipal User);
}