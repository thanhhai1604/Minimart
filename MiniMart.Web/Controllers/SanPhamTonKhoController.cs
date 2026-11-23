using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class SanPhamTonKhoController : Controller
    {
        private readonly MiniMartDbContext _context;

        public SanPhamTonKhoController(MiniMartDbContext context)
        {
            _context = context;
        }

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        private string? LayMaSTCuaToi()
        {
            var maST = HttpContext.Session.GetString("MaST");
            return string.IsNullOrWhiteSpace(maST) || maST.Equals("ALL", StringComparison.OrdinalIgnoreCase)
                ? null
                : maST.Trim();
        }

        // ==========================================
        // 1. INDEX: Danh sách sản phẩm tồn kho
        // ==========================================
        public async Task<IActionResult> Index(string? maST = null, bool khuyenMai = false)
        {
            if (!CoQuyen("WEB_HH_SANPHAMTONKHO"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var filterMaST = maSTCuaToi ?? (string.IsNullOrWhiteSpace(maST) ? null : maST.Trim());

            ViewBag.GioiHanSieuThi = maSTCuaToi != null;
            ViewBag.SelectedMaST = filterMaST;
            ViewBag.ChiKhuyenMai = khuyenMai;

            ViewBag.DanhSachSieuThi = await _context.SieuThis
                .AsNoTracking()
                .OrderBy(s => s.TenST)
                .ToListAsync();

            var query = _context.SanPhamTonKhos
                .AsNoTracking()
                .Include(t => t.SieuThiNavigation)
                .Include(t => t.SanPhamNavigation)
                    .ThenInclude(sp => sp!.DanhMucNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterMaST))
            {
                query = query.Where(t => t.MaST == filterMaST);
            }

            var danhSach = await query
                .OrderBy(t => t.MaST)
                .ThenBy(t => t.MaSP)
                .ToListAsync();

            return View(danhSach);
        }

        // ==========================================
        // 2. ĐỒNG BỘ SẢN PHẨM VÀO TỒN KHO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongBoSanPham(string? maST = null)
        {
            if (!CoQuyen("WEB_HH_SPTK_DONGBO"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var targetMaST = maSTCuaToi ?? (string.IsNullOrWhiteSpace(maST) ? null : maST.Trim());

            List<string> danhSachMaST;
            if (!string.IsNullOrEmpty(targetMaST))
            {
                var tonTaiST = await _context.SieuThis
                    .AsNoTracking()
                    .AnyAsync(s => s.MaST == targetMaST);

                if (!tonTaiST)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy siêu thị có mã '{targetMaST}'.";
                    return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
                }

                danhSachMaST = new List<string> { targetMaST };
            }
            else
            {
                danhSachMaST = await _context.SieuThis
                    .AsNoTracking()
                    .Select(s => s.MaST)
                    .ToListAsync();
            }

            var danhSachMaSP = await _context.SanPhams
                .AsNoTracking()
                .Select(sp => sp.MaSP)
                .ToListAsync();

            if (!danhSachMaST.Any() || !danhSachMaSP.Any())
            {
                TempData["ErrorMessage"] = "Chưa có dữ liệu siêu thị hoặc sản phẩm để đồng bộ.";
                return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
            }

            var daTonTaiSet = (await _context.SanPhamTonKhos
                .AsNoTracking()
                .Where(t => danhSachMaST.Contains(t.MaST))
                .Select(t => new { t.MaST, t.MaSP })
                .ToListAsync())
                .Select(x => $"{x.MaST}||{x.MaSP}")
                .ToHashSet();

            var banGhiMoi = new List<SanPhamTonKho>();
            foreach (var stId in danhSachMaST)
            {
                foreach (var spId in danhSachMaSP)
                {
                    if (!daTonTaiSet.Contains($"{stId}||{spId}"))
                    {
                        banGhiMoi.Add(new SanPhamTonKho
                        {
                            MaST = stId,
                            MaSP = spId,
                            GiaNhap = 0.00m,
                            GiaGoc = 0.00m,
                            GiaBan = 0.00m,
                            SoLuongTon = 0,
                            TrangThaiKinhDoanh = 0
                        });
                    }
                }
            }

            if (banGhiMoi.Count > 0)
            {
                await _context.SanPhamTonKhos.AddRangeAsync(banGhiMoi);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = maSTCuaToi != null
                    ? $"Đã đồng bộ {banGhiMoi.Count} sản phẩm từ danh mục vào tồn kho siêu thị {maSTCuaToi}."
                    : $"Đã đồng bộ {banGhiMoi.Count} dòng sản phẩm từ danh mục vào tồn kho.";
            }
            else
            {
                TempData["SuccessMessage"] = "Tất cả sản phẩm đã được đồng bộ đầy đủ lên tồn kho.";
            }

            return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
        }

        // ==========================================
        // 3. EDIT (GET): Xem chi tiết / Cập nhật tồn kho
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string maST, string maSP)
        {
            if (!CoQuyen("WEB_HH_SPTK_XEM"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrWhiteSpace(maST) || string.IsNullOrWhiteSpace(maSP))
            {
                return NotFound();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && !string.Equals(maST.Trim(), maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            var targetMaST = maSTCuaToi ?? maST.Trim();
            var targetMaSP = maSP.Trim();

            var item = await _context.SanPhamTonKhos
                .AsNoTracking()
                .Include(t => t.SieuThiNavigation)
                .Include(t => t.SanPhamNavigation)
                    .ThenInclude(sp => sp!.DanhMucNavigation)
                .FirstOrDefaultAsync(t => t.MaST == targetMaST && t.MaSP == targetMaSP);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // ==========================================
        // 4. EDIT (POST): Cập nhật tồn kho
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string maST, string maSP, SanPhamTonKho model)
        {
            if (!CoQuyen("WEB_HH_SPTK_CAPNHAT"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var targetMaST = maSTCuaToi ?? (string.IsNullOrWhiteSpace(model.MaST) ? maST?.Trim() : model.MaST.Trim());
            var targetMaSP = string.IsNullOrWhiteSpace(model.MaSP) ? maSP?.Trim() : model.MaSP.Trim();

            if (string.IsNullOrWhiteSpace(targetMaST) || string.IsNullOrWhiteSpace(targetMaSP))
            {
                return NotFound();
            }

            if (maSTCuaToi != null && !string.Equals(targetMaST, maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            var existing = await _context.SanPhamTonKhos
                .Include(t => t.SieuThiNavigation)
                .Include(t => t.SanPhamNavigation)
                    .ThenInclude(sp => sp!.DanhMucNavigation)
                .FirstOrDefaultAsync(t => t.MaST == targetMaST && t.MaSP == targetMaSP);

            if (existing == null)
            {
                return NotFound();
            }

            if (model.GiaNhap < 0)
            {
                ModelState.AddModelError(nameof(SanPhamTonKho.GiaNhap), "Giá nhập không được nhỏ hơn 0.");
            }
            if (model.GiaGoc < 0)
            {
                ModelState.AddModelError(nameof(SanPhamTonKho.GiaGoc), "Giá gốc không được nhỏ hơn 0.");
            }
            if (model.GiaBan < 0)
            {
                ModelState.AddModelError(nameof(SanPhamTonKho.GiaBan), "Giá bán không được nhỏ hơn 0.");
            }
            if (model.SoLuongTon < 0)
            {
                ModelState.AddModelError(nameof(SanPhamTonKho.SoLuongTon), "Số lượng tồn không được nhỏ hơn 0.");
            }

            if (!ModelState.IsValid)
            {
                model.SieuThiNavigation = existing.SieuThiNavigation;
                model.SanPhamNavigation = existing.SanPhamNavigation;
                return View(model);
            }

            existing.GiaNhap = Math.Round(model.GiaNhap, 2, MidpointRounding.AwayFromZero);
            existing.GiaGoc = Math.Round(model.GiaGoc, 2, MidpointRounding.AwayFromZero);
            existing.GiaBan = Math.Round(model.GiaBan, 2, MidpointRounding.AwayFromZero);
            existing.SoLuongTon = model.SoLuongTon;
            existing.TrangThaiKinhDoanh = model.TrangThaiKinhDoanh == 1 ? 1 : 0;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật tồn kho thành công!";
                return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.SanPhamTonKhos.AnyAsync(e => e.MaST == targetMaST && e.MaSP == targetMaSP))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // ==========================================
        // 5. DELETE (POST): Xóa sản phẩm tồn kho
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string maST, string maSP)
        {
            if (!CoQuyen("WEB_HH_SPTK_XOA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrWhiteSpace(maST) || string.IsNullOrWhiteSpace(maSP))
            {
                return NotFound();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && !string.Equals(maST.Trim(), maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            var targetMaST = maSTCuaToi ?? maST.Trim();
            var targetMaSP = maSP.Trim();

            var item = await _context.SanPhamTonKhos
                .Include(t => t.SanPhamNavigation)
                .FirstOrDefaultAsync(t => t.MaST == targetMaST && t.MaSP == targetMaSP);

            if (item == null)
            {
                return NotFound();
            }

            try
            {
                _context.SanPhamTonKhos.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sản phẩm tồn kho thành công!";
            }
            catch (DbUpdateException)
            {
                var tenSP = item.SanPhamNavigation?.TenSP?.Trim() ?? targetMaSP;
                TempData["ForeignKeyDeleteError"] =
                    $"Không thể xóa sản phẩm tồn kho \"{tenSP}\" ({targetMaSP} - {targetMaST}) vì còn các thông tin liên quan như chi tiết phiếu nhập...";
            }

            return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
        }
    }
}