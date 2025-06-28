using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tourify_Frontend.Controllers
{
    // Nếu chỉ cho Sub-Company mới xem được, thêm [Authorize(Roles="SubCompany")]
    [Authorize(Roles = "sub-company")]
    public class DashboardController : Controller
    {
        // GET: /Dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}
