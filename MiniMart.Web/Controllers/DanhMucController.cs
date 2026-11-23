using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMart.Web.Controllers
{
    public class DanhMucController(MiniMartDbContext context) : Controller
    {
        private bool CheckPermission(string parentPermission, string childPermission = "")
        {
            var userPermissions = HttpContext.Session.GetString("UserPermissions") ?? "";
            bool isAdmin = HttpContext.Session.GetString("ChucVu") == "Admin";

            bool hasPerm = isAdmin || userPermissions.Contains(parentPermission) || (!string.IsNullOrEmpty(childPermission) && userPermissions.Contains(childPermission));

            if (!hasPerm)
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
            }
            return hasPerm;
        }

        // 1. INDEX: Danh sách danh mục kèm số lượng sản phẩm
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_DANHMUC", "WEB_HH_DM_XEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            // Dùng .Include để nạp danh sách sản phẩm giúp đếm chính xác số lượng
            var danhMucs = await context.DanhMucs
                .Include(dm => dm.SanPhams)
                .ToListAsync();

            return View(danhMucs);
        }

        // 2. CREATE (GET): Hiển thị form thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_DM_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            return View();
        }

        // CREATE (POST): Xử lý lưu dữ liệu thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            if (!CheckPermission("WEB_HH_DM_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (await context.DanhMucs.AnyAsync(d => d.MaDM == danhMuc.MaDM))
            {
                ModelState.AddModelError("MaDM", "Mã danh mục này đã tồn tại trên hệ thống!");
            }

            if (ModelState.IsValid)
            {
                context.DanhMucs.Add(danhMuc);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // 3. EDIT (GET): Hiển thị form chỉnh sửa & danh sách sản phẩm thuộc danh mục
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_DANHMUC", "WEB_HH_DM_XEM") && !CheckPermission("WEB_HH_DM_SUA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var danhMuc = await context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound();

            // Nạp danh sách sản phẩm kèm thông tin thuế của từng sản phẩm
            ViewBag.DanhSachSanPham = await context.SanPhams
                .Where(sp => sp.MaDM == id)
                .Include(sp => sp.ThueNavigation)
                .ToListAsync();

            return View(danhMuc);
        }

        // EDIT (POST): Xử lý cập nhật thông tin danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DanhMuc danhMuc)
        {
            if (!CheckPermission("WEB_HH_DM_SUA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (id != danhMuc.MaDM) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(danhMuc);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.DanhMucs.Any(e => e.MaDM == danhMuc.MaDM)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi model, nạp lại danh sách sản phẩm để render view
            ViewBag.DanhSachSanPham = await context.SanPhams
                .Where(sp => sp.MaDM == id)
                .Include(sp => sp.ThueNavigation)
                .ToListAsync();

            return View(danhMuc);
        }

        // 4. DELETE (POST): Xóa danh mục (kiểm tra nếu đang chứa sản phẩm thì chặn)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CheckPermission("WEB_HH_DM_XOA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            var danhMuc = await context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                bool hasSanPham = await context.SanPhams.AnyAsync(sp => sp.MaDM == id);
                if (hasSanPham)
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục đang chứa sản phẩm!";
                    return RedirectToAction(nameof(Index));
                }

                context.DanhMucs.Remove(danhMuc);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}