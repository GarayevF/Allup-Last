using Allup.Models;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class BlogController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            return Ok(blog);
        }
    }
}
