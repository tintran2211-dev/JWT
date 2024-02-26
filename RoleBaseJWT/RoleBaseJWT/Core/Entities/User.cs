using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleBaseJWT.Core.Entities
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address {  get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public IList<string>? Roles { get; set; }
    }
}
