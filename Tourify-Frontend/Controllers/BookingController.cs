using Microsoft.AspNetCore.Mvc;

namespace Tourify_Frontend.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
