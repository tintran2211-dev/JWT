namespace RoleBaseJWT.Core.Entities
{
    public class BaseEntity<TID>
    {
        public TID Id { get; set; }
        public DateTime CreatedDate { get; set; }  
        public DateTime UpdatedDate { get; set;}
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
