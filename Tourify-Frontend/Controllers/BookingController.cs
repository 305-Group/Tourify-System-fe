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
// cái này để dành cho booking và xem chi tiết booking(mấy bạn có thể tự tạo thêm controller trong quá trinh làm nha)