using System.ComponentModel.DataAnnotations.Schema;

namespace Allup.Models
{
    public class OrderItem : BaseEntity
    {
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        [Column(TypeName = "money")]
        public double? Price { get; set; }
        public int? Count { get; set; }
    }
}
