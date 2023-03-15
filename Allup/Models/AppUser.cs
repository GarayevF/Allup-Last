using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(100)]
        public string? Name { get; set; }
        [StringLength(100)]
        public string? SurName { get; set; }
        public bool IsActive { get; set; }
        public List<Basket>? Baskets { get; set; }
        public IEnumerable<Review>? Reviews { get; set; }
    }
}
