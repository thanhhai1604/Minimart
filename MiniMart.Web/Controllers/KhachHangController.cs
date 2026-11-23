using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class KhachHangController : Controller
    {
        // Đổi mã khách hàng vãng lai mặc định tại duy nhất dòng này
        private const string MaKhachHangVangLai = "KHVLMINIMART";
        private readonly MiniMartDbContext _context;

        public KhachHangController(MiniMartDbContext context)
        {
            _context = context;
        }

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private bool LaKhachHangVangLai(string? maKH) =>
            !string.IsNullOrWhiteSpace(maKH) &&
            string.Equals(maKH.Trim(), MaKhachHangVangLai, StringComparison.OrdinalIgnoreCase);

        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        // 1. INDEX: Danh sách khách hàng
        public async Task<IActionResult> Index()
        {
            if (!CoQuyen("WEB_KHACHHANG") && !CoQuyen("WEB_KH_XEM"))
            {
                return ChanDirectLink();
            }

            var danhSachKH = await _context.KhachHangs
                .AsNoTracking()
                .OrderBy(k => k.MaKH)
                .ToListAsync();

            return View(danhSachKH);
        }

        // 2. CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            if (!CoQuyen("WEB_KH_THEM"))
            {
                return ChanDirectLink();
            }
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang kh)
        {
            if (!CoQuyen("WEB_KH_THEM"))
            {
                return ChanDirectLink();
            }

            if (await _context.KhachHangs.AnyAsync(k => k.MaKH == kh.MaKH))
            {
                ModelState.AddModelError("MaKH", "Mã khách hàng này đã tồn tại trong hệ thống.");
            }

            // Khi tạo mới, các điểm mặc định là 0
            kh.DiemTichLuy = 0;
            kh.DiemSuDung = 0;
            kh.DiemKhaDung = 0;

            if (ModelState.IsValid)
            {
                _context.Add(kh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(kh);
        }

        // 3. EDIT (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!CoQuyen("WEB_KH_XEM") && !CoQuyen("WEB_KH_SUA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return NotFound();

            ViewBag.LaKhachVangLai = LaKhachHangVangLai(kh.MaKH);

            // Nạp danh sách hóa đơn mà khách hàng này đã mua kèm thông tin siêu thị
            ViewBag.DanhSachHoaDon = await _context.HoaDons
                .Where(h => h.MaKH == id)
                .Include(h => h.SieuThiNavigation)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(kh);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhachHang kh)
        {
            if (!CoQuyen("WEB_KH_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != kh.MaKH) return NotFound();

            var khGoc = await _context.KhachHangs.FindAsync(id);
            if (khGoc == null) return NotFound();

            bool laKhachVangLai = LaKhachHangVangLai(khGoc.MaKH);
            ViewBag.LaKhachVangLai = laKhachVangLai;

            if (laKhachVangLai)
            {
                kh.TenKH = khGoc.TenKH;
                ModelState.Remove(nameof(KhachHang.TenKH));
            }

            // Giữ nguyên điểm tích lũy và điểm sử dụng cũ, không cho phép client ghi đè trực tiếp điểm tích lũy/sử dụng trừ khi qua logic nghiệp vụ
            kh.DiemTichLuy = khGoc.DiemTichLuy;
            kh.DiemSuDung = khGoc.DiemSuDung;
            kh.DiemKhaDung = khGoc.DiemTichLuy - khGoc.DiemSuDung; // Tự động tính lại điểm khả dụng

            ModelState.Remove(nameof(KhachHang.DiemTichLuy));
            ModelState.Remove(nameof(KhachHang.DiemSuDung));
            ModelState.Remove(nameof(KhachHang.DiemKhaDung));

            if (ModelState.IsValid)
            {
                try
                {
                    if (!laKhachVangLai)
                    {
                        khGoc.TenKH = kh.TenKH;
                    }
                    khGoc.SoDT = kh.SoDT;
                    khGoc.Email = kh.Email;
                    khGoc.DiaChi = kh.DiaChi;

                    _context.Update(khGoc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhachHangs.Any(e => e.MaKH == kh.MaKH)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (laKhachVangLai)
            {
                kh.TenKH = khGoc.TenKH;
            }

            ViewBag.DanhSachHoaDon = await _context.HoaDons
                .Where(h => h.MaKH == id)
                .Include(h => h.SieuThiNavigation)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(kh);
        }

        // 4. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_KH_XOA"))
            {
                return ChanDirectLink();
            }

            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh != null)
            {
                if (LaKhachHangVangLai(kh.MaKH))
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng vãng lai mặc định của hệ thống!";
                    return RedirectToAction(nameof(Index));
                }

                bool hasHoaDon = await _context.HoaDons.AnyAsync(h => h.MaKH == id);
                if (hasHoaDon)
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng đã phát sinh hóa đơn mua hàng!";
                    return RedirectToAction(nameof(Index));
                }

                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}