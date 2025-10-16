using Microsoft.AspNetCore.Mvc;

namespace Compliance_Tracker.Controllers
{
    public class ComponentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Demo()
        {
            return View();
        }
    }
}
