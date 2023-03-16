using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels.BasketViewModels;
using Allup.ViewModels.OrderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Controllers
{
    [Authorize(Roles = "Member")]
    public class OrderController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public OrderController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            AppUser appUser = await _userManager.Users
                .Include(u => u.Baskets.Where(b => b.IsDeleted == false)).ThenInclude(b => b.Product)
                .Include(u => u.Addresses.Where(a => a.IsMain && a.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if(appUser.Baskets == null || appUser.Baskets.Count() <= 0)
            {
                return RedirectToAction("index", "home");
            }

            if(appUser.Addresses == null || appUser.Addresses.Count() <= 0)
            {
                return RedirectToAction("profile", "account");
            }


            OrderVM orderVM = new OrderVM
            {
                Order = new Order
                {
                    Name = appUser.Name,
                    SurName = appUser.SurName,
                    Email = appUser.Email,
                    Country = appUser.Addresses.First().Country,
                    City = appUser.Addresses.First().City,
                    State = appUser.Addresses.First().State,
                    PostalCode = appUser.Addresses.First().PostalCode,
                },
                Baskets = appUser.Baskets
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            return Ok(order);
        }
    }
}
