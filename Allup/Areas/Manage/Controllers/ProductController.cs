using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Product> queries = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductTags.Where(pt=>pt.IsDeleted == false))
                .ThenInclude(pt=>pt.Tag)
                .Where(p => p.IsDeleted == false);

            return View(PageNatedList<Product>.Create(queries, pageIndex, 3));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(c => c.IsDeleted== false).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted== false).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(c => c.IsDeleted== false).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            return Ok();
        }
    }
}
