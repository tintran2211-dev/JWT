using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoleBaseJWT.Core.Entities;
using RoleBaseJWT.Core.IServices;
using RoleBaseJWT.Core.Model.Log;

namespace RoleBaseJWT.Core.Services;

public class LogServices : ILogServices
{
    private readonly AppDBContext.AppDBContext _context;

    public LogServices(AppDBContext.AppDBContext context)
    {
        _context = context;
    }

    public async Task SaveNewLog(string UserName, string Description)
    {
        var newLog = new Log()
        {
            UserName = UserName,
            Description = Description
        };
        await _context.Logs.AddAsync(newLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GetLogDTO>> GetLogsAsync()
    {
        var logs = await _context.Logs
            .Select(x => new GetLogDTO
            {
                CreateAt = x.CreatedDate,
                UserName = x.UserName,
                Description = x.Description
            }).OrderByDescending(x => x.CreateAt).ToListAsync();
        return logs;
    }
    
    public async Task<IEnumerable<GetLogDTO>> GetMyLogsAsync(ClaimsPrincipal User)
    {
        var logs = await _context.Logs
            .Where(x => x.UserName == User.Identity.Name)
            .Select(x => new GetLogDTO
            {
                CreateAt = x.CreatedDate,
                UserName = x.UserName,
                Description = x.Description
            }).OrderByDescending(x => x.CreateAt).ToListAsync();
        return logs;
    }
}