using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleBaseJWT.Core.Constants;

namespace RoleBaseJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        [HttpGet]
        [Route("get-public")]
        public IActionResult GetPublicData()
        {
            return Ok("Public data");
        }
        
        [HttpGet]
        [Route("get-user-role")]
        [Authorize(Roles = StaticUserRoles.USER)]
        public IActionResult GetUserData()
        {
            return Ok("User Role data");
        }
        
        [HttpGet]
        [Route("get-manager-role")]
        [Authorize(Roles = StaticUserRoles.MANAGER)]
        public IActionResult GetManagerData()
        {
            return Ok("Manager Role data");
        }
        
        [HttpGet]
        [Route("get-admin-role")]
        [Authorize(Roles = StaticUserRoles.ADMIN)]
        public IActionResult GetAdminData()
        {
            return Ok("Admin Role data");
        }
        
        [HttpGet]
        [Route("get-owner-role")]
        [Authorize(Roles = StaticUserRoles.OWNER)]
        public IActionResult GetOwnerData()
        {
            return Ok("Owner Role data");
        }
    }
}