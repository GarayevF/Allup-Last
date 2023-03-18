using Allup.Enums;
using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class Order : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public int No { get; set; }
        [StringLength(500)]
        public string? Comment { get; set; }
        public OrderType Status { get; set; }
        [StringLength(100)]
        public string? Name { get; set; }
        [StringLength(100)]
        public string? SurName { get; set; }
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        [StringLength(100)]
        public string? Country { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        [StringLength(100)]
        public string? State { get; set; }
        [StringLength(100)]
        public string? PostalCode { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
    }
}
