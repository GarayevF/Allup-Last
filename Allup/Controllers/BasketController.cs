﻿using Allup.DataAccessLayer;
using Allup.Models;
using Allup.ViewModels.BasketViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Allup.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            List<BasketVM> basketVMs = null;

            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == basketVM.Id);

                if (product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Image = product.MainImage;
                    basketVM.ExTax = product.ExTax;
                }
            }

            return View(basketVMs);
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) return BadRequest();

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();


            string cookie = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (string.IsNullOrWhiteSpace(cookie))
            {
                basketVMs = new List<BasketVM>()
                {
                    new BasketVM
                    {
                        Id = (int)id,
                        Count = 1
                    }
                };  
            }
            else
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

                if (basketVMs.Exists(p=>p.Id == id))
                {
                    basketVMs.Find(b => b.Id == id).Count += 1;
                }
                else
                {
                    basketVMs.Add(new BasketVM { Id = (int)id, Count = 1 });
                }
            }

            if(User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                AppUser appUser = await _userManager.Users
                    .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if(appUser.Baskets != null && appUser.Baskets.Count() > 0)
                {
                    if(appUser.Baskets.Any(b => b.ProductId == id))
                    {
                        appUser.Baskets.FirstOrDefault(b => b.ProductId == id).Count = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                    }
                    else
                    {
                        Basket basket = new Basket
                        {
                            ProductId = id,
                            Count = 1
                        };

                        appUser.Baskets.Add(basket);
                    }
                }
                else
                {
                    Basket basket = new Basket
                    {
                        ProductId = id,
                        Count = 1
                    };

                    appUser.Baskets.Add(basket);
                }

                await _context.SaveChangesAsync();
            }

            cookie = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", cookie);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == basketVM.Id);

                if (product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Image = product.MainImage;
                    basketVM.ExTax = product.ExTax;
                }
            }

            return PartialView("_BasketCartPartial", basketVMs);

        }

        public async Task<IActionResult> IncreaseCount(int? id)
        {
            if (id == null) return BadRequest();

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();


            string cookie = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (string.IsNullOrWhiteSpace(cookie))
            {
                return BadRequest();
            }

            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

            if (!basketVMs.Exists(p => p.Id == id))
            {
                return BadRequest();
            }

            basketVMs.Find(b => b.Id == id).Count += 1;


            cookie = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", cookie);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == basketVM.Id);

                if (product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Image = product.MainImage;
                    basketVM.ExTax = product.ExTax;
                }
            }

            return PartialView("_BasketMainPartial", basketVMs);

        }

        public async Task<IActionResult> MainBasket()
        {
            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == basketVM.Id);

                if (product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Image = product.MainImage;
                    basketVM.ExTax = product.ExTax;
                }
            }

            return PartialView("_BasketMainPartial", basketVMs);
        }


        public async Task<IActionResult> RemoveBasket(int? id)
        {
            if (id == null) return BadRequest();

            if (!await _context.Products.AnyAsync(p => p.IsDeleted == false && p.Id == id)) return NotFound();

            string cookie = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

                if (basketVMs.Exists(p => p.Id == id))
                {
                    if (basketVMs.Find(b => b.Id == id).Count > 1)
                    {
                        basketVMs.Find(b => b.Id == id).Count -= 1;
                    }
                    else
                    {
                        basketVMs.Remove(basketVMs.FirstOrDefault(b => b.Id == id));
                    }
                    
                }
                else
                {
                    return BadRequest();
                    //basketVMs.Remove(new BasketVM { Id = (int)id, Count = 1 });
                }
            }

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.IsDeleted == false && p.Id == basketVM.Id);

                if (product != null)
                {
                    basketVM.Title = product.Title;
                    basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                    basketVM.Image = product.MainImage;
                    basketVM.ExTax = product.ExTax;
                }
            }

            cookie = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", cookie);

            return PartialView("_BasketCartPartial", basketVMs);
        }

        public async Task<IActionResult> GetBasket()
        {
            string basket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

            return Json(basketVMs);
        }
    }
}
