using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class Review : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        [Range(1, 5)]
        public int Start { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(1000)]
        public string Comment { get; set; }
    }
}
