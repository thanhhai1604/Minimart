using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly MiniMartDbContext _context;

        public NhaCungCapController(MiniMartDbContext context)
        {
            _context = context;
        }

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        // 1. INDEX: Danh sách Nhà cung cấp
        public async Task<IActionResult> Index()
        {
            if (!CoQuyen("WEB_QL_NHACUNGCAP") && !CoQuyen("WEB_QL_NCC_XEM"))
            {
                return ChanDirectLink();
            }

            var danhSachNCC = await _context.NhaCungCaps
                .AsNoTracking()
                .OrderBy(n => n.MaNCC)
                .ToListAsync();

            return View(danhSachNCC);
        }

        // 2. CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            if (!CoQuyen("WEB_QL_NCC_THEM"))
            {
                return ChanDirectLink();
            }
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaCungCap ncc)
        {
            if (!CoQuyen("WEB_QL_NCC_THEM"))
            {
                return ChanDirectLink();
            }

            if (await _context.NhaCungCaps.AnyAsync(n => n.MaNCC == ncc.MaNCC))
            {
                ModelState.AddModelError("MaNCC", "Mã nhà cung cấp này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(ncc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới nhà cung cấp thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(ncc);
        }

        // 3. EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!CoQuyen("WEB_QL_NCC_XEM") && !CoQuyen("WEB_QL_NCC_SUA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null) return NotFound();

            return View(ncc);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhaCungCap ncc)
        {
            if (!CoQuyen("WEB_QL_NCC_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != ncc.MaNCC) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ncc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NhaCungCaps.Any(e => e.MaNCC == ncc.MaNCC)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(ncc);
        }

        // 4. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_QL_NCC_XOA"))
            {
                return ChanDirectLink();
            }

            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc != null)
            {
                _context.NhaCungCaps.Remove(ncc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}