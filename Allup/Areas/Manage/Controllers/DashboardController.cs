using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Areas.Manage.Controllers
{
    public class DashboardController : Controller
    {
        [Area("manage")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
