using System.Security.Claims;
using RoleBaseJWT.Core.Model.General;
using RoleBaseJWT.Core.Model.Log;
using RoleBaseJWT.Core.Model.Message;

namespace RoleBaseJWT.Core.IServices;

public interface IMessageServices
{
    Task<GeneralServiceResponseDTO> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDTO createMessageDto);
    Task<IEnumerable<GetMessageDTO>> GetMessagesAsync();
    Task<IEnumerable<GetMessageDTO>> GetMyMessagesAsync(ClaimsPrincipal User);
}