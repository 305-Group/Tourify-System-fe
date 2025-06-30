using Microsoft.AspNetCore.Mvc;

namespace Tourify_Frontend.Controllers
{
    public class UserController : Controller
    {
        public IActionResult ViewProfile()
        {
            return View();
        }
    }
}