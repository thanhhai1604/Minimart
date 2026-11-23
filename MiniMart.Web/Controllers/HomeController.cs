using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;

namespace MiniMart.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly MiniMartDbContext _context;

        public HomeController(MiniMartDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // TỔNG QUAN (cần quyền WEB_TONGQUAN)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Chưa đăng nhập -> về trang đăng nhập
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("MaNV")))
            {
                return RedirectToAction("Login", "Account");
            }

            // Không có quyền Tổng quan -> popup 2 rồi chuyển về Tài khoản của tôi
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_TONGQUAN"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            ViewBag.TongSoSanPham = await _context.SanPhams.CountAsync();
            ViewBag.TongSoNhanVien = await _context.NhanViens.CountAsync();
            ViewBag.TongSoSieuThi = await _context.SieuThis.CountAsync();

            return View(new List<dynamic>());
        }

        // ==========================================
        // TRANG LỖI (404 và các mã lỗi khác)
        // Được gọi bởi app.UseStatusCodePagesWithReExecute("/Home/NotFound/{0}") trong Program.cs
        // KHÔNG gắn phân quyền cho action này để người chưa đăng nhập vẫn thấy trang lỗi.
        // ==========================================
        [ActionName("NotFound")]
        public IActionResult HienThiLoi(int? id)
        {
            var maLoi = id ?? 404;

            Response.StatusCode = maLoi;   // giữ đúng mã trạng thái
            ViewBag.MaLoi = maLoi;

            return View("404");            // Views/Home/404.cshtml (view có sẵn của bạn)
        }
    }
}
