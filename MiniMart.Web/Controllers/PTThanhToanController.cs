using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class PTThanhToanController : Controller
    {
        private readonly MiniMartDbContext _context;

        public PTThanhToanController(MiniMartDbContext context)
        {
            _context = context;
        }

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        // ==========================================
        // 1. INDEX: Danh sách phương thức thanh toán
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền cha hoặc quyền xem chi tiết
            if (!CoQuyen("WEB_HT_PTTT") && !CoQuyen("WEB_HT_PTTT_XEM"))
            {
                return ChanDirectLink();
            }

            var danhSach = await _context.PTThanhToans
                .AsNoTracking()
                .OrderBy(p => p.MaPTTT)
                .ToListAsync();

            return View(danhSach);
        }

        // ==========================================
        // 2. CREATE (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            if (!CoQuyen("WEB_HT_PTTT_THEM"))
            {
                return ChanDirectLink();
            }
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PTThanhToan pttt)
        {
            if (!CoQuyen("WEB_HT_PTTT_THEM"))
            {
                return ChanDirectLink();
            }

            if (await _context.PTThanhToans.AnyAsync(p => p.MaPTTT == pttt.MaPTTT))
            {
                ModelState.AddModelError("MaPTTT", "Mã phương thức thanh toán này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(pttt);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới phương thức thanh toán thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(pttt);
        }

        // ==========================================
        // 3. EDIT (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // Kiểm tra quyền xem hoặc quyền sửa
            if (!CoQuyen("WEB_HT_PTTT_XEM") && !CoQuyen("WEB_HT_PTTT_SUA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var pttt = await _context.PTThanhToans.FindAsync(id);
            if (pttt == null) return NotFound();

            return View(pttt);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PTThanhToan pttt)
        {
            if (!CoQuyen("WEB_HT_PTTT_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != pttt.MaPTTT) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pttt);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật phương thức thanh toán thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PTThanhToanExists(pttt.MaPTTT)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(pttt);
        }

        // ==========================================
        // 4. DELETE (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_HT_PTTT_XOA"))
            {
                return ChanDirectLink();
            }

            var pttt = await _context.PTThanhToans.FindAsync(id);
            if (pttt != null)
            {
                _context.PTThanhToans.Remove(pttt);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa phương thức thanh toán thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PTThanhToanExists(string id)
        {
            return _context.PTThanhToans.Any(e => e.MaPTTT == id);
        }
    }
}