using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly MiniMartDbContext _context;

        public SanPhamController(MiniMartDbContext context)
        {
            _context = context;
        }

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        private async Task LoadDropdownDataAsync(string? selectedMaDM = null, string? selectedMaThue = null)
        {
            var listDM = await _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToListAsync();
            var listThue = await _context.Thues.OrderBy(t => t.TenThue).ToListAsync();

            ViewBag.DanhSachDanhMuc = new SelectList(listDM, "MaDM", "TenDanhMuc", selectedMaDM);
            ViewBag.DanhSachThue = new SelectList(listThue, "MaThue", "TenThue", selectedMaThue);
        }

        // 1. INDEX: Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            if (!CoQuyen("WEB_HH_SANPHAM") && !CoQuyen("WEB_HH_SP_XEM"))
            {
                return ChanDirectLink();
            }

            var danhSachSP = await _context.SanPhams
                .AsNoTracking()
                .Include(s => s.DanhMucNavigation)
                .Include(s => s.ThueNavigation)
                .OrderBy(s => s.MaSP)
                .ToListAsync();

            return View(danhSachSP);
        }

        // 2. CREATE (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!CoQuyen("WEB_HH_SP_THEM"))
            {
                return ChanDirectLink();
            }

            await LoadDropdownDataAsync();
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sp)
        {
            if (!CoQuyen("WEB_HH_SP_THEM"))
            {
                return ChanDirectLink();
            }

            if (await _context.SanPhams.AnyAsync(s => s.MaSP == sp.MaSP))
            {
                ModelState.AddModelError("MaSP", "Mã sản phẩm này đã tồn tại trong hệ thống.");
            }
            if (await _context.SanPhams.AnyAsync(s => s.UPC == sp.UPC))
            {
                ModelState.AddModelError("UPC", "Mã UPC này đã được sử dụng cho sản phẩm khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(sp);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownDataAsync(sp.MaDM, sp.MaThue);
            return View(sp);
        }

        // 3. EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!CoQuyen("WEB_HH_SP_XEM") && !CoQuyen("WEB_HH_SP_SUA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();

            await LoadDropdownDataAsync(sp.MaDM, sp.MaThue);
            return View(sp);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SanPham sp)
        {
            if (!CoQuyen("WEB_HH_SP_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != sp.MaSP) return NotFound();

            if (await _context.SanPhams.AnyAsync(s => s.UPC == sp.UPC && s.MaSP != id))
            {
                ModelState.AddModelError("UPC", "Mã UPC này đã được sử dụng cho sản phẩm khác.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sp);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanPhams.Any(e => e.MaSP == sp.MaSP)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownDataAsync(sp.MaDM, sp.MaThue);
            return View(sp);
        }

        // 4. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_HH_SP_XOA"))
            {
                return ChanDirectLink();
            }

            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                try
                {
                    _context.SanPhams.Remove(sp);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                }
                catch (DbUpdateException)
                {
                    TempData["ForeignKeyDeleteError"] =
                        $"Không thể xóa sản phẩm \"{sp.TenSP}\" ({sp.MaSP}) vì còn liên quan đến các thông tin khác trong hệ thống như tồn kho, chi tiết phiếu nhập...";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}