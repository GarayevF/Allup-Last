using Allup.Models;
using Allup.ViewModels.BasketViewModels;

namespace Allup.ViewModels.OrderViewModels
{
    public class OrderVM
    {
        public Order Order { get; set; }
        public IEnumerable<Basket> Baskets { get; set; }
    }
}
