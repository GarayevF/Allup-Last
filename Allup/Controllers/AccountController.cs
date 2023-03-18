using Allup.ViewModels.AccountViewModels;
using Allup.Models;
using Allup.ViewModels.BasketViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Allup.DataAccessLayer;
using Allup.ViewModels.AccountViewModels;
using MimeKit;
using MailKit.Net.Smtp;
using Allup.ViewModels;
using Microsoft.Extensions.Options;

namespace Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _con;
        private readonly SmtpSetting _smtpSetting;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            AppDbContext context, IConfiguration con, IOptions<SmtpSetting> smtpSetting)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _con = con;
            _smtpSetting = smtpSetting.Value;
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

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Member");

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            string url = Url.Action("EmailConfirm", "Account", new {id = appUser.Id, token = token}, HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Shared", "_EmailConfirm.cshtml");
            string templateContent = await System.IO.File.ReadAllTextAsync(templatePath);
            templateContent = templateContent.Replace("{{email}}", appUser.Email);
            templateContent = templateContent.Replace("{{url}}", url);

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Email Confirmation";
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = templateContent
            };

            using (SmtpClient smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }

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

            Microsoft.AspNetCore.Identity.SignInResult signInResult = null;

            if (appUser.EmailConfirmed)
            {
                signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);
            }
            else
            {
                ModelState.AddModelError("", "emailinizi tesdiqleyin");
                return View();
            }

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
                .Include(u => u.Orders.Where(o => o.IsDeleted == false)).ThenInclude(o => o.OrderItems.Where(oi => oi.IsDeleted == false)).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses,
                Address = new Address(),
                Orders = appUser.Orders,
                AccountVM = new AccountVM
                {
                    Name = appUser.Name,
                    SurName = appUser.SurName,
                    Email = appUser.Email,
                    UserName = appUser.UserName,
                }
            };

            return View(profileVM);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> UpdateProfile(AccountVM accountVM)
        {
            TempData["Tab"] = "Account";

            AppUser appUser = await _userManager.Users
                .Include(u => u.Addresses.Where(a => a.IsDeleted == false))
                .Include(u => u.Orders.Where(o => o.IsDeleted == false)).ThenInclude(o => o.OrderItems.Where(oi => oi.IsDeleted == false)).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ProfileVM profileVM = new ProfileVM
            {
                Addresses = appUser.Addresses,
                Address = new Address(),
                Orders = appUser.Orders,
                AccountVM = accountVM
            };

            if (!ModelState.IsValid)
            {
                return View("Profile", profileVM);
            }


            appUser.Name = accountVM.Name;
            appUser.SurName = accountVM.SurName;

            if (appUser.NormalizedUserName != accountVM.UserName.Trim().ToUpperInvariant())
            {
                if (await _userManager.Users.AnyAsync(u => u.NormalizedUserName == accountVM.UserName.Trim().ToUpperInvariant() && u.Id != appUser.Id))
                {
                    ModelState.AddModelError("UserName", $"UserName {accountVM.UserName} already exists.");
                    return View("Profile", profileVM);
                }
                else
                {
                    appUser.UserName = accountVM.UserName.Trim();
                }
            }

            if (appUser.NormalizedEmail != accountVM.Email.Trim().ToUpperInvariant())
            {
                if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == accountVM.Email.Trim().ToUpperInvariant() && u.Id != appUser.Id))
                {
                    ModelState.AddModelError("Email", $"Email {accountVM.Email} already exists.");
                    return View("Profile", profileVM);
                }
                else
                {
                    appUser.Email = accountVM.Email.Trim();
                }
            }

            appUser.Email = accountVM.Email;
            appUser.UserName = accountVM.UserName;

            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View("Profile", profileVM);
            }

            if (!string.IsNullOrWhiteSpace(accountVM.CurrentPassword))
            {
                if(await _userManager.CheckPasswordAsync(appUser, accountVM.CurrentPassword))
                {
                    string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

                    identityResult = await _userManager.ResetPasswordAsync(appUser, token, accountVM.Password);

                    if (!identityResult.Succeeded)
                    {
                        foreach (IdentityError identityError in identityResult.Errors)
                        {
                            ModelState.AddModelError("", identityError.Description);
                        }

                        return View("Profile", profileVM);
                    }
                }
                else
                {
                    ModelState.AddModelError("CurrentPassword", "CurrentPassword yanlisdir");
                    return View("Profile", profileVM);
                }
            }
            

            await _signInManager.SignInAsync(appUser, true);

            TempData["Success"] = "Hesabiniz ugurla yenilendi";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirm(string? id, string? token)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            if (string.IsNullOrWhiteSpace(token)) return BadRequest();

            AppUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, token);

            if (!identityResult.Succeeded) return BadRequest();

            TempData["Success"] = $"{appUser.Email} tesdiqlendi";

            return RedirectToAction(nameof(Login));
        }
    }
}
