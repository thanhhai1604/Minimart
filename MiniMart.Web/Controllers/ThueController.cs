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
    public class ThueController(MiniMartDbContext context) : Controller
    {
        // Kiểm tra quyền hạn linh hoạt: Có quyền cha hoặc quyền con đều được phép
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

        // 1. INDEX: Danh sách thuế (Chỉ cần có quyền cha WEB_HH_THUE là vào xem được danh sách)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_THUE", "WEB_HH_THUE_XEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            var listThue = await context.Thues.ToListAsync();
            return View(listThue);
        }

        // 2. CREATE: Thêm mới (Kiểm tra quyền WEB_HH_THUE_THEM)
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_THUE_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Thue thue)
        {
            if (!CheckPermission("WEB_HH_THUE_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (await context.Thues.AnyAsync(t => t.MaThue == thue.MaThue))
            {
                ModelState.AddModelError("MaThue", "Mã thuế này đã tồn tại trên hệ thống!");
                return View(thue);
            }

            if (ModelState.IsValid)
            {
                context.Thues.Add(thue);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới mức thuế thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(thue);
        }

        // 3. EDIT: Sửa (Kiểm tra quyền WEB_HH_THUE_SUA)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_HH_THUE_SUA", "WEB_HH_THUE_XEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var thue = await context.Thues.FindAsync(id);
            if (thue == null) return NotFound();

            return View(thue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Thue thue)
        {
            if (!CheckPermission("WEB_HH_THUE_SUA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (id != thue.MaThue) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(thue);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thuế thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.Thues.Any(e => e.MaThue == thue.MaThue)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(thue);
        }

        // 4. DELETE: Xóa (Kiểm tra quyền WEB_HH_THUE_XOA)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CheckPermission("WEB_HH_THUE_XOA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            var thue = await context.Thues.FindAsync(id);
            if (thue != null)
            {
                context.Thues.Remove(thue);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa mức thuế thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}