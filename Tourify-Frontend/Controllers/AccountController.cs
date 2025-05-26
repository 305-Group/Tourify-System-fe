using Microsoft.AspNetCore.Mvc;

namespace Tourify_Frontend.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
