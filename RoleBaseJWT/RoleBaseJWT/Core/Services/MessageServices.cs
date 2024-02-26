using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoleBaseJWT.Core.Entities;
using RoleBaseJWT.Core.IServices;
using RoleBaseJWT.Core.Model.General;
using RoleBaseJWT.Core.Model.Log;
using RoleBaseJWT.Core.Model.Message;

namespace RoleBaseJWT.Core.Services;

public class MessageServices : IMessageServices
{
    private readonly AppDBContext.AppDBContext _context;
    private readonly ILogServices _logServices;
    private readonly UserManager<User> _userManager;

    public MessageServices(AppDBContext.AppDBContext context, ILogServices logServices, UserManager<User> userManager)
    {
        _context = context;
        _logServices = logServices;
        _userManager = userManager;
    }

    public async Task<GeneralServiceResponseDTO> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDTO createMessageDto)
    {
        if (User.Identity.Name == createMessageDto.ReceiverUserName)
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 400,
                Message = "Sender and Receiver can not be same"
            };
        
        var isReceiverUserNamevalid = _userManager.Users.Any(x => x.UserName == createMessageDto.ReceiverUserName);
        if(!isReceiverUserNamevalid)
            return new GeneralServiceResponseDTO()
            {
                IsSucceed = false,
                StatusCode = 400,
                Message = "Receiver UserName is not valid"
            };

        Message newMessage = new Message()
        {
            SenderUserName = User.Identity.Name,
            ReceiverUserName = createMessageDto.ReceiverUserName,
            Text = createMessageDto.Text
        };

        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();
        await _logServices.SaveNewLog(User.Identity.Name, "Send Message");

        return new GeneralServiceResponseDTO()
        {
            IsSucceed = true,
            StatusCode = 200,
            Message = "Message Saved Successfully"   
        };
    }

    public async Task<IEnumerable<GetMessageDTO>> GetMessagesAsync()
    {
        var messages = await _context.Messages
            .Select(x => new GetMessageDTO()
            {
                Id = x.Id,
                SenderUserName = x.SenderUserName,
                ReceiverUserName = x.ReceiverUserName,
                Text = x.Text,
                CreatedAt = x.CreatedDate
            }).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return messages;
    }

    public async Task<IEnumerable<GetMessageDTO>> GetMyMessagesAsync(ClaimsPrincipal User)
    {
        var loggedInUser = User.Identity.Name;
        
        var messages = await _context.Messages
            .Where(x => x.SenderUserName == loggedInUser || x.ReceiverUserName == loggedInUser)
            .Select(x => new GetMessageDTO()
            {
                Id = x.Id,
                SenderUserName = x.SenderUserName,
                ReceiverUserName = x.ReceiverUserName,
                Text = x.Text,
                CreatedAt = x.CreatedDate
            }).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return messages;
    }
}