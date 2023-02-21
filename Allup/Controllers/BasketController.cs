using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels.BasketViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Allup.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) return BadRequest();

            //if (await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();


            string cookie = HttpContext.Request.Cookies["basket"];

            if(string.IsNullOrWhiteSpace(cookie))
            {
                List<BasketVM> basketVMs = new List<BasketVM>()
                {
                    new BasketVM
                    {
                        Id = (int)id,
                        Count = 1
                    }
                };

                string basket = JsonConvert.SerializeObject(basketVMs);
                HttpContext.Response.Cookies.Append("basket", basket);

                return PartialView("_CardPartial", basketVMs);
            }
            else
            {
                List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

                if (basketVMs.Exists(p=>p.Id == id))
                {
                    basketVMs.Find(b => b.Id == id).Count = 1;
                }
                else
                {
                    basketVMs.Add(new BasketVM { Id = (int)id, Count = 1 });
                }

                string basket = JsonConvert.SerializeObject(basketVMs);
                HttpContext.Response.Cookies.Append("basket", basket);

                return PartialView("_CardPartial", basketVMs);
            }

        }

        public async Task<IActionResult> GetBasket()
        {
            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

            return Json(basketVMs);
        }
    }
}
