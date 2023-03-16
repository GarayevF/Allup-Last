using Allup.ViewModels.AccountViewModels;
using Allup.Models;
using Allup.ViewModels.BasketViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Allup.DataAccessLayer;

namespace Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            AppUser appUser = new AppUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                Name = registerVM.Name,
                SurName = registerVM.SurName,
            };

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if(!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Member");

            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            AppUser appUser = await _userManager.Users
                .Include(u=>u.Baskets.Where(b=>b.IsDeleted == false))
                .FirstOrDefaultAsync(u=>u.NormalizedEmail == loginVM.Email.Trim().ToUpperInvariant());

            if (appUser == null)
            {
                ModelState.AddModelError("", "Email or Password Is Incorrect");
                return View(loginVM);
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);

            if(appUser.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Hesabiniz bloklanib");
                return View(loginVM);
            }

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email or Password Is Incorrect");
                return View(loginVM);
            }

            string cookie = HttpContext.Request.Cookies["basket"];

            if (appUser.Baskets != null && appUser.Baskets.Count() > 0)
            {
                List<BasketVM> basketVMs = new List<BasketVM>();

                foreach (Basket basket in appUser.Baskets)
                {
                    BasketVM basketVM = new BasketVM
                    {
                        Id = (int)basket.ProductId,
                        Count = basket.Count
                    };

                    basketVMs.Add(basketVM);
                }

                cookie = JsonConvert.SerializeObject(basketVMs);

                HttpContext.Response.Cookies.Append("basket", cookie);
            }
            else
            {
                HttpContext.Response.Cookies.Append("", cookie);
            }


            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.Users
                .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses,
                Address = new Address()
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> AddAddress(Address address)
        {
            AppUser appUser = await _userManager.Users
                .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TempData["Tab"] = "Address";

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses,
                Address = address
            };

            if (!ModelState.IsValid)
            {
                TempData["ModelError"] = "Error";
                return View("Profile", profileVM);
            }

            if(address.IsMain == false && (appUser.Addresses == null || appUser.Addresses.Count() <= 0))
            {
                address.IsMain = true;
            }
            
            if (address.IsMain && (appUser.Addresses == null || appUser.Addresses.Count() <= 0) && appUser.Addresses.Any(a => a.IsMain))
            {
                appUser.Addresses.FirstOrDefault(a => a.IsMain && a.IsDeleted == false).IsMain = false;
            }

            address.CreatedAt = DateTime.UtcNow.AddHours(4);
            address.CreatedBy = $"{appUser.Name} {appUser.SurName}";

            appUser.Addresses.Add(address);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }
    }
}
