using Microsoft.AspNetCore.Mvc;

namespace WebsiteMedical.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        [Route("Admin")]
        public IActionResult Index()
        {
            return RedirectToAction("Campaigns");
        }

        // Menu 1: Quản lý Campaigns
        [HttpGet]
        [Route("Admin/Campaigns")]
        public IActionResult Campaigns()
        {
            return View();
        }

        // Menu 2: Quản lý User
        [HttpGet]
        [Route("Admin/Users")]
        public IActionResult Users()
        {
            return View();
        }
    }
}

