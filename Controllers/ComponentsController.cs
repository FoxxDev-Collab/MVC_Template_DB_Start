using Microsoft.AspNetCore.Mvc;

namespace HLE.Template.Controllers;

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
