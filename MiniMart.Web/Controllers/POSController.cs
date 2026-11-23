using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class POSController : Controller
    {
        private readonly MiniMartDbContext _context;

        public POSController(MiniMartDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // HÀM DÙNG CHUNG
        // ==========================================

        // Có quyền này không?
        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        // Chặn truy cập trực tiếp bằng đường dẫn: bật popup 2 rồi chuyển về Tài khoản của tôi
        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        // Siêu thị của tài khoản đang đăng nhập.
        // null = tài khoản không thuộc siêu thị nào (MaST rỗng) -> xem được TẤT CẢ siêu thị
        private string? LayMaSTCuaToi()
        {
            var maST = HttpContext.Session.GetString("MaST");
            return string.IsNullOrEmpty(maST) || maST == "ALL" ? null : maST;
        }

        // Nạp danh sách siêu thị cho ô chọn (tài khoản thuộc 1 siêu thị chỉ thấy siêu thị của mình)
        private async Task LoadSieuThiBagAsync(string? selectedMaST = null)
        {
            var query = _context.SieuThis.Where(st => st.TrangThai == 1);

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null)
            {
                query = query.Where(st => st.MaST == maSTCuaToi);
            }

            // View dùng cờ này để khóa ô chọn siêu thị khi tài khoản chỉ thuộc 1 siêu thị
            ViewBag.GioiHanSieuThi = maSTCuaToi != null;

            var listST = await query.ToListAsync();
            ViewBag.DanhSachSieuThi = new SelectList(listST, "MaST", "TenST", selectedMaST);
        }

        // ==========================================
        // 1. INDEX: Danh sách POS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            if (!CoQuyen("WEB_QL_POS") && !CoQuyen("WEB_QL_POS_XEM"))
            {
                return ChanDirectLink();
            }

            var query = _context.POS
                .AsNoTracking()
                .Include(p => p.SieuThiNavigation)
                .AsQueryable();

            // Tài khoản thuộc 1 siêu thị: chỉ thấy POS của siêu thị đó. MaST rỗng: thấy tất cả.
            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null)
            {
                query = query.Where(p => p.MaST == maSTCuaToi);
            }

            var listPos = await query
                .OrderBy(p => p.MaST)
                .ThenBy(p => p.MaPOS)
                .ToListAsync();

            return View(listPos);
        }

        // ==========================================
        // 2. CREATE (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!CoQuyen("WEB_QL_POS_THEM"))
            {
                return ChanDirectLink();
            }

            // Tài khoản thuộc 1 siêu thị: tự chọn sẵn siêu thị của mình
            var maSTCuaToi = LayMaSTCuaToi();
            await LoadSieuThiBagAsync(maSTCuaToi);
            return View(new POS { MaST = maSTCuaToi ?? string.Empty });
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(POS pos)
        {
            if (!CoQuyen("WEB_QL_POS_THEM"))
            {
                return ChanDirectLink();
            }

            // Tài khoản thuộc 1 siêu thị chỉ được thêm POS cho siêu thị của mình (bỏ qua giá trị gửi lên)
            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null)
            {
                pos.MaST = maSTCuaToi;
                ModelState.Remove(nameof(POS.MaST));
            }

            if (await _context.POS.AnyAsync(p => p.MaPOS == pos.MaPOS && p.MaST == pos.MaST))
            {
                ModelState.AddModelError("MaPOS", "Mã POS này đã tồn tại trong siêu thị được chọn!");
            }

            if (ModelState.IsValid)
            {
                _context.POS.Add(pos);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới thiết bị POS thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSieuThiBagAsync(pos.MaST);
            return View(pos);
        }

        // ==========================================
        // 3. EDIT (GET) - khóa kép: id = MaPOS, maST = MaST
        // Xem chi tiết chỉ dành cho tài khoản có quyền "Xem POS"
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id, string maST)
        {
            if (!CoQuyen("WEB_QL_POS_XEM"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(maST)) return NotFound();

            // Không được xem POS của siêu thị khác
            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && maST != maSTCuaToi)
            {
                return ChanDirectLink();
            }

            var pos = await _context.POS
                .AsNoTracking()
                .Include(p => p.SieuThiNavigation)
                .FirstOrDefaultAsync(p => p.MaPOS == id && p.MaST == maST);

            if (pos == null) return NotFound();

            return View(pos);
        }

        // EDIT (POST): chỉ cho sửa Tên POS và Trạng thái; Mã POS và Siêu thị luôn lấy từ database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string maST, POS pos)
        {
            if (!CoQuyen("WEB_QL_POS_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != pos.MaPOS || maST != pos.MaST) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && maST != maSTCuaToi)
            {
                return ChanDirectLink();
            }

            // Lấy bản ghi gốc theo cặp khóa (MaPOS + MaST) lấy từ đường dẫn, không tin dữ liệu form
            var posGoc = await _context.POS
                .Include(p => p.SieuThiNavigation)
                .FirstOrDefaultAsync(p => p.MaPOS == id && p.MaST == maST);

            if (posGoc == null) return NotFound();

            if (ModelState.IsValid)
            {
                posGoc.TenPOS = pos.TenPOS;
                posGoc.TrangThai = pos.TrangThai;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin POS thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Có lỗi: hiển thị lại trang với dữ liệu vừa nhập (không lưu vào database)
            posGoc.TenPOS = pos.TenPOS;
            posGoc.TrangThai = pos.TrangThai;
            return View(posGoc);
        }

        // ==========================================
        // 4. DELETE (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, string maST)
        {
            if (!CoQuyen("WEB_QL_POS_XOA"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && maST != maSTCuaToi)
            {
                return ChanDirectLink();
            }

            var pos = await _context.POS.FirstOrDefaultAsync(p => p.MaPOS == id && p.MaST == maST);
            if (pos != null)
            {
                _context.POS.Remove(pos);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thiết bị POS thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
