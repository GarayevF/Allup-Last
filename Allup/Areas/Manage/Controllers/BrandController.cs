using Allup.DataAccessLayer;
using Allup.Models;
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

        public async Task<IActionResult> Index()
        {
            IEnumerable<Brand> brands = await _context.Brands.Include(b=>b.Products).Where(b=>b.IsDeleted == false).ToListAsync();
            return View(brands);
        }

        [HttpGet]
        public IActionResult Create() 
        { 
            return View(); 
        }

        [HttpPost]
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
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
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



        [HttpPost]
        public async Task<IActionResult> Delete(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }
            if (!await _context.Brands.AnyAsync(b => b.IsDeleted == false))
            {
                return BadRequest();
            }

            brand.IsDeleted = true;

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
    }
}
