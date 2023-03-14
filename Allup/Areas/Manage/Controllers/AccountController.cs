using Allup.Areas.Manage.ViewModels.AccountViewModels;
using Allup.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class AccountController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid) return View(registerVM);

            AppUser appUser = new AppUser 
            { 
                Name = registerVM.Name,
                SurName = registerVM.SurName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };

            //if(await _userManager.Users.AnyAsync(u=>u.NormalizedUserName == registerVM.UserName.Trim().ToUpperInvariant()))
            //{
            //    ModelState.AddModelError("UserName", $"{registerVM.UserName} already taken");
            //    return View(registerVM);
            //}

            //if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == registerVM.Email.Trim().ToUpperInvariant()))
            //{
            //    ModelState.AddModelError("Email", $"{registerVM.Email} already taken");
            //    return View(registerVM);
            //}

            

            IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);

            if(!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, "Admin");

            return RedirectToAction(nameof(Login));
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            AppUser appUser = await _userManager.FindByEmailAsync(loginVM.Email);

            if(appUser == null)
            {
                ModelState.AddModelError("", "Email ve ya Sifre Yanlisdir");
                return View(loginVM);
            }

            if(!appUser.IsActive)
            {
                ModelState.AddModelError("", "Hesabiniz deaktiv olunub");
                return View(loginVM);
            }

            //if(await _userManager.CheckPasswordAsync(appUser, loginVM.Password))
            //{
            //    ModelState.AddModelError("", "Email ve ya Sifre Yanlisdir");
            //    return View(loginVM);
            //}

            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
                .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);

            if (appUser.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Hesabiniz bloklanib");
                return View(loginVM);
            }

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email ve ya Sifre Yanlisdir");
                return View(loginVM);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "manage" });
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            ProfileVM profileVM = new ProfileVM
            {
                Name = appUser.Name,
                SurName = appUser.SurName,
                UserName = appUser.UserName,
                Email = appUser.Email,
            };

            return View(profileVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
        {
            if(!ModelState.IsValid)
            {
                return View(profileVM);
            }

            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            appUser.Name = profileVM.Name;
            appUser.SurName = profileVM.SurName;

            if(appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant())
            {
                if (await _userManager.Users.AnyAsync(u => u.NormalizedUserName == profileVM.UserName.Trim().ToUpperInvariant() && u.Id != appUser.Id))
                {
                    ModelState.AddModelError("UserName", $"UserName {profileVM.UserName} already exists.");
                    return View(profileVM);
                }
                else
                {
                    appUser.UserName = profileVM.UserName.Trim();
                }
            }

            if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant())
            {
                if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == profileVM.Email.Trim().ToUpperInvariant() && u.Id != appUser.Id))
                {
                    ModelState.AddModelError("Email", $"Email {profileVM.Email} already exists.");
                    return View(profileVM);
                }
                else
                {
                    appUser.Email = profileVM.Email.Trim();
                }
            }

            appUser.Email = profileVM.Email;
            appUser.UserName = profileVM.UserName;

            IdentityResult identityResult = await _userManager.UpdateAsync(appUser);

            if(!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                }

                return View(profileVM);
            }

            if(!string.IsNullOrWhiteSpace(profileVM.CurrentPassword) && await _userManager.CheckPasswordAsync(appUser, profileVM.CurrentPassword))
            {
                string token =  await _userManager.GeneratePasswordResetTokenAsync(appUser);

                identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.Password);

                if (!identityResult.Succeeded)
                {
                    foreach (IdentityError identityError in identityResult.Errors)
                    {
                        ModelState.AddModelError("", identityError.Description);
                    }

                    return View(profileVM);
                }
            }
            else
            {
                ModelState.AddModelError("CurrentPassword", "CurrentPassword yanlisdir");
                return View(profileVM);
            }

            await _signInManager.SignInAsync(appUser, true);

            return RedirectToAction("Index", "Dashboard", new { area = "Manage" });
        }


        #region Create Roles
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));

        //    return Ok("rollar yaradildi");
        //}

        //public async Task<IActionResult> CreateSuperAdmin()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Name = "Super",
        //        SurName = "Admin",
        //        UserName = "SuperAdmin",
        //        Email = "superadmin@gmail.com"
        //    };

        //    await _userManager.CreateAsync(appUser, "SuperAdmin229");
        //    await _userManager.AddToRoleAsync(appUser, "SuperAdmin");

        //    return Ok("super admin yaradildi");
        //}
        #endregion
    }
}
