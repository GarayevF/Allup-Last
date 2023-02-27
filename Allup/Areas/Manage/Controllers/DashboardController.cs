using Microsoft.AspNetCore.Mvc;

namespace Allup.Areas.Manage.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
