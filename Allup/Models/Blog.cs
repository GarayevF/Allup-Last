using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class Blog : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        [StringLength(255)]
        public string? Image { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        public string Desc { get; set; }
    }
}
