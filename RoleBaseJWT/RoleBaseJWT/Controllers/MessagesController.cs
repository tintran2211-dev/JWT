using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleBaseJWT.Core.Constants;
using RoleBaseJWT.Core.IServices;
using RoleBaseJWT.Core.Model.Message;

namespace RoleBaseJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageServices _messageServices;

        public MessagesController(IMessageServices messageServices)
        {
            _messageServices = messageServices;
        }
        
        //Route => Tao tin nhan moi gui toi cac user khac
        [HttpPost]
        [Route("create")]
        [Authorize]
        public async Task<IActionResult> CreateNewMessage([FromBody]CreateMessageDTO createMessageDto)
        {
            var result = await _messageServices.CreateNewMessageAsync(User, createMessageDto);
            if(result.IsSucceed)
                return Ok(result.Message);
            return StatusCode(result.StatusCode, result.Message);
        }
        
        //Route => Nhận tất cả tin nhắn cho người dùng hiện tại, với tư cách là người gửi hoặc người nhận
        [HttpGet]
        [Route("mine")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetMessageDTO>>> GetMyMessages()
        {
            var messages = await _messageServices.GetMyMessagesAsync(User);
            return Ok(messages);
        }
        
        //Route => Nhận tất cả tin nhắn có quyền truy cập của chủ sở hữu và quyền truy cập của quản trị viên
        [HttpGet]
        [Authorize(Roles = StaticUserRoles.OwnerAdmin)]
        public async Task<ActionResult<IEnumerable<GetMessageDTO>>> GetMessages()
        {
            var messages = await _messageServices.GetMessagesAsync();
            return Ok(messages);
        }
    }
}