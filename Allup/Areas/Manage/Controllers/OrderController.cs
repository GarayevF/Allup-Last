using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int pageIndex = 1)
        {
            IQueryable<Order> orders = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.IsDeleted == false);

            return View(PageNatedList<Order>.Create(orders, pageIndex, 5, 5));
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Order order = await _context.Orders
                .Include(o => o.OrderItems.Where(oi => oi.IsDeleted == false)).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.IsDeleted == false);

            if(order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOrder(int? id, Order order)
        {
            if(id == null) return BadRequest();

            if(id != order.Id) return BadRequest();

            Order dbOrder = await _context.Orders
                .Include(o => o.OrderItems.Where(oi => oi.IsDeleted == false)).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsDeleted == false);

            if (dbOrder == null) return NotFound();

            if((int)order.Status < 0 || (int)order.Status > 4)
            {
                ModelState.AddModelError("Status", "duzgun secim edin");
                return View("Detail", dbOrder);
            }

            dbOrder.Status = order.Status;
            dbOrder.Comment = order.Comment;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
