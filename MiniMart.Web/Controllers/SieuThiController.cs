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
    public class SieuThiController(MiniMartDbContext context) : Controller
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

        // 1. INDEX: Danh sách siêu thị
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_QL_SIEUTHI", "WEB_QL_ST_XEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            var listSieuThi = await context.SieuThis.ToListAsync();
            return View(listSieuThi);
        }

        // 2. CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_QL_ST_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SieuThi sieuThi)
        {
            if (!CheckPermission("WEB_QL_ST_THEM"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (await context.SieuThis.AnyAsync(s => s.MaST == sieuThi.MaST))
            {
                ModelState.AddModelError("MaST", "Mã siêu thị này đã tồn tại trên hệ thống!");
            }

            if (ModelState.IsValid)
            {
                context.SieuThis.Add(sieuThi);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới siêu thị thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(sieuThi);
        }

        // 3. EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString("TenDN") == null)
                return RedirectToAction("Login", "Account");

            if (!CheckPermission("WEB_QL_SIEUTHI", "WEB_QL_ST_SUA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var sieuThi = await context.SieuThis.FindAsync(id);
            if (sieuThi == null) return NotFound();

            return View(sieuThi);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SieuThi sieuThi)
        {
            if (!CheckPermission("WEB_QL_ST_SUA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            if (id != sieuThi.MaST) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(sieuThi);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin siêu thị thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.SieuThis.Any(e => e.MaST == sieuThi.MaST)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sieuThi);
        }

        // 4. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CheckPermission("WEB_QL_ST_XOA"))
            {
                return RedirectToAction("Profile", "Account");
            }

            var sieuThi = await context.SieuThis.FindAsync(id);
            if (sieuThi != null)
            {
                context.SieuThis.Remove(sieuThi);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa siêu thị thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}