using Microsoft.AspNetCore.Mvc;

namespace WebsiteMedical.Controllers
{
    public class StaffController : Controller
    {
        [HttpGet]
        [Route("Staff")]
        public IActionResult Index()
        {
            return RedirectToAction("DutySchedule");
        }

        // Menu 1: Xem phân công trực của Campaigns
        [HttpGet]
        [Route("Staff/DutySchedule")]
        public IActionResult DutySchedule()
        {
            return View();
        }

        // Menu: Tạo hồ sơ khám
        [HttpGet]
        [Route("Staff/CreateRecord")]
        public IActionResult CreateRecord()
        {
            return View();
        }

        // Menu 2: Quản lý hồ sơ khám
        [HttpGet]
        [Route("Staff/MedicalRecords")]
        public IActionResult MedicalRecords()
        {
            return View();
        }

        // Menu 3: Tiếp nhận & Check-in
        [HttpGet]
        [Route("Staff/CheckIn")]
        public IActionResult CheckIn()
        {
            return View();
        }
    }
}

