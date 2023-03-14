using Allup.DataAccessLayer;
using Allup.Extensions;
using Allup.Helpers;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Category> query = _context.Categories
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .Where(p => p.IsDeleted == false && p.IsMain)
                .OrderByDescending(c => c.Id);

            //ViewBag.TotalCount = (int)Math.Ceiling((decimal)categories.Count() / 3);
            //ViewBag.PageIndex = pageIndex;

            return View(PageNatedList<Category>.Create(query, pageIndex, 3, 8));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .Include(c => c.Children.Where(ca => ca.IsDeleted == false)).ThenInclude(ch => ch.Products.Where(p => p.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.MainCategories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            if (!ModelState.IsValid) return View();

            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower() == category.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", $"Bu adda {category.Name} category movcuddu");
                return View(category);
            }

            if (category.IsMain)
            {
                if (category.File?.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("File", "Uygun type deyil");
                    return View();
                }
                if ((category.File?.Length / 1024) > 300)
                {
                    ModelState.AddModelError("File", "Size uygun deyil");
                    return View();
                }
                
                category.Image = await category.File.CreateFileAsync(_env, "assets", "images");
                category.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Parent mutleq secilmelidir");
                    return View(category);
                }

                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Id == category.ParentId && c.IsMain))
                {
                    ModelState.AddModelError("ParentId", "Parent mutleq secilmelidir");
                    return View(category);
                }

                category.Image = null;
            }

            category.Name = category.Name.Trim();
            category.CreatedAt = DateTime.UtcNow.AddHours(4);
            category.CreatedBy = "System";

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null) return BadRequest();

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

            ViewBag.MainCategories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (!ModelState.IsValid) return View(category);

            if (id == null) return BadRequest();

            if (id != category.Id) return BadRequest();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

            if (await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower() == category.Name.Trim().ToLower() && c.Id != category.Id))
            {
                ModelState.AddModelError("Name", $"Bu adda {category.Name} category movcuddur");
                return View(category);
            }

            if (dbCategory.IsMain == category.IsMain)
            {
                ModelState.AddModelError("IsMain", "Categorynin veziyyeti deyisdirile bilmez");
                return View(dbCategory);
            }

            if (dbCategory.IsMain && category.File != null)
            {
                if (category.File.CheckFileContenttype("image/jpeg"))
                {
                    ModelState.AddModelError("File", "Uygun type deyil");
                    return View();
                }
                if (category.File.CheckFileLength(300))
                {
                    ModelState.AddModelError("File", "Size uygun deyil");
                    return View();
                }

                FileHelper.DeleteFile(dbCategory.Image, _env, "assets", "images");

                dbCategory.Image = await category.File.CreateFileAsync(_env, "assets", "images"); ;
            }
            else
            {
                if (category.ParentId != dbCategory.ParentId)
                {
                    if (category.ParentId == null)
                    {
                        ModelState.AddModelError("ParentId", "Parent mutleq secilmelidir");
                        return View(category);
                    }

                    if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Id == category.ParentId && c.IsMain))
                    {
                        ModelState.AddModelError("ParentId", "Parent mutleq secilmelidir");
                        return View(category);
                    }

                    dbCategory.ParentId = category.ParentId;
                }
            }

            dbCategory.Name = category.Name.Trim();
            dbCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);
            dbCategory.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .Include(c => c.Children.Where(ca => ca.IsDeleted == false)).ThenInclude(ch => ch.Products.Where(p => p.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories
                .Include(c => c.Children.Where(ca => ca.IsDeleted == false)).ThenInclude(ch => ch.Products.Where(p => p.IsDeleted == false))
                .Include(c => c.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null) return NotFound();

            if(category.Children != null && category.Children.Count() > 0)
            {
                foreach (Category child in category.Children)
                {
                    child.IsDeleted = true;
                    child.DeletedBy = "System";
                    child.DeletedAt = DateTime.UtcNow.AddHours(4);

                    if(child.Products != null && child.Products.Count() > 0)
                    {
                        foreach (Product product in child.Products)
                        {
                            product.CategoryId = null;
                        }
                    }
                }
            }


            if (category.Products != null && category.Products.Count() > 0)
            {
                foreach (Product product in category.Products)
                {
                    product.CategoryId = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(category.Image))
            {
                FileHelper.DeleteFile(category.Image, _env, "assets", "images");
            }

            category.IsDeleted = true;
            category.DeletedBy = "System";
            category.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
