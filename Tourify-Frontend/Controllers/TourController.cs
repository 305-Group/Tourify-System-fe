using Microsoft.AspNetCore.Mvc;

namespace Tourify_Frontend.Controllers
{
    public class TourController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}



// này danh cho xem chi tiết tour (mấy bạn có thể tự tạo thêm controller trong quá trinh làm nha)