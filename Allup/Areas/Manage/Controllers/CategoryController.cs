using Allup.DataAccessLayer;
using Allup.Models;
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

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
                .Include(c=>c.Products.Where(p=>p.IsDeleted == false))
                .Where(p=>p.IsDeleted == false && p.IsMain)
                .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _context.Categories.Where(c=>c.IsDeleted == false && c.IsMain).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.MainCategories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            if (!ModelState.IsValid) return View();

            if(await _context.Categories.AnyAsync(c=>c.IsDeleted == false && c.Name.ToLower() == category.Name.Trim().ToLower()))
            {
                ModelState.AddModelError("Name", $"Bu adda {category.Name} category movcuddu");
                return View(category);
            }

            if(category.IsMain)
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

                int lastIndex = category.File.FileName.LastIndexOf(".");

                string name = category.File.FileName.Substring(lastIndex);

                string fileName = $"{DateTime.UtcNow.ToString("yyyMMddHHmmssfff")}_{Guid.NewGuid().ToString()}{name}";


                string fullPath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    await category.File.CopyToAsync(stream);
                }
                category.Image = fileName;
                category.ParentId = null;
            }
            else
            {
                if(category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Parent mutleq secilmelidir");
                    return View(category);
                }

                if(!await _context.Categories.AnyAsync(c=>c.IsDeleted == false && c.Id == category.ParentId && c.IsMain))
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

            Category category = await _context.Categories.FirstOrDefaultAsync(c=> c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

            ViewBag.MainCategories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (!ModelState.IsValid) return View();

            if(id == null) return BadRequest();

            if (id != category.Id) return BadRequest();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null) return NotFound();

            if(await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.Name.ToLower() == category.Name.Trim().ToLower() && c.Id != category.Id))
            {
                ModelState.AddModelError("Name", $"Bu adda {category.Name} category movcuddur");
                return View(category);
            }

            if(dbCategory.IsMain == category.IsMain)
            {
                ModelState.AddModelError("IsMain", "Categorynin veziyyeti deyisdirile bilmez");
                return View(category);
            }

            if (dbCategory.IsMain && category.File != null)
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

                int lastIndex = category.File.FileName.LastIndexOf(".");

                string name = category.File.FileName.Substring(lastIndex);

                string fileName = $"{DateTime.UtcNow.ToString("yyyMMddHHmmssfff")}_{Guid.NewGuid().ToString()}{name}";

                string fullPath = Path.Combine(_env.WebRootPath, "assets", "images", dbCategory.Image);

                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                fullPath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    await category.File.CopyToAsync(stream);
                }

                

                dbCategory.Image = fileName;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
