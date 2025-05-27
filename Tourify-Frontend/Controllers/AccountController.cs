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
// cái này dùng cho những cái xử lý dành cho auth (mấy bạn có thể tự tạo thêm controller trong quá trinh làm nha)