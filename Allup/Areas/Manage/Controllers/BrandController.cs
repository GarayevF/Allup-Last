using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Brand> query = _context.Brands
                .Include(b=>b.Products)
                .Where(b=>b.IsDeleted == false);

            return View(PageNatedList<Brand>.Create(query, pageIndex, 3));
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Detail(int? id)
        {
            if(id == null) return BadRequest();

            Brand brand = await _context.Brands.Include(b=>b.Products.Where(p=>p.IsDeleted == false))
                .FirstOrDefaultAsync(b=>b.Id == id && b.IsDeleted == false);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create() 
        { 
            return View(); 
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create(Brand brand)
        {
            if(!ModelState.IsValid)
            {
                return View(brand);
            }
            if (await _context.Brands.AnyAsync(b=>b.IsDeleted == false && b.Name.ToLower() == brand.Name.Trim().ToLower())) 
            {
                ModelState.AddModelError("Name", $"Bu adda {brand.Name} movcuddur");
            }

            brand.Name = brand.Name.Trim();
            brand.CreatedAt = DateTime.UtcNow.AddHours(4);
            brand.CreatedBy= "System";

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (brand == null) return NotFound();

            return View(brand);

        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }
            if (!await _context.Brands.AnyAsync(b => b.IsDeleted == false))
            {
                return BadRequest();
            }

            brand.Name = brand.Name.Trim();

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }



        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null) return BadRequest();

            Brand brand = await _context.Brands.Include(b => b.Products.Where(p => p.IsDeleted == false))
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (brand == null) return NotFound();

            brand.IsDeleted = true;
            brand.DeletedBy = "system";
            brand.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            IEnumerable<Brand> brands = await _context.Brands.Include(b=>b.Products).Where(b=>b.IsDeleted == false).ToListAsync();

            return PartialView("_BrandIndexPartial", brands);

        }
    }
}
