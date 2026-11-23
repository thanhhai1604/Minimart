using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly MiniMartDbContext _context;
        private readonly IPasswordHasher<NhanVien> _passwordHasher;

        public NhanVienController(MiniMartDbContext context, IPasswordHasher<NhanVien> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Kiểm tra quyền
        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        // Chặn Direct Link
        private IActionResult ChanDirectLink()
        {
            HttpContext.Session.SetString("ShowDirectLinkError", "true");
            return RedirectToAction("Profile", "Account");
        }

        // Lấy MaST của tài khoản đang đăng nhập (null nếu không thuộc siêu thị nào / quản lý toàn hệ thống)
        private string? LayMaSTCuaToi()
        {
            var maST = HttpContext.Session.GetString("MaST");
            return string.IsNullOrEmpty(maST) || maST == "ALL" ? null : maST;
        }

        // Nạp danh sách siêu thị (Giới hạn nếu tài khoản thuộc 1 siêu thị)
        private async Task LoadSieuThiBagAsync(string? selectedMaST = null)
        {
            var query = _context.SieuThis.Where(st => st.TrangThai == 1);
            var maSTCuaToi = LayMaSTCuaToi();

            if (!string.IsNullOrEmpty(maSTCuaToi))
            {
                query = query.Where(st => st.MaST == maSTCuaToi);
            }

            ViewBag.GioiHanSieuThi = !string.IsNullOrEmpty(maSTCuaToi);
            var listST = await query.ToListAsync();
            ViewBag.DanhSachSieuThi = new SelectList(listST, "MaST", "TenST", selectedMaST);
        }

        // Nạp danh sách Vai trò (Ẩn vai trò Quản lý hệ thống nếu tài khoản thuộc siêu thị)
        private async Task LoadVaiTroBagAsync(string? selectedMaVaiTro = null)
        {
            var query = _context.VaiTros.AsQueryable();
            var maSTCuaToi = LayMaSTCuaToi();

            if (!string.IsNullOrEmpty(maSTCuaToi))
            {
                query = query.Where(vt => !vt.TenVaiTro.ToLower().Contains("quản lý hệ thống") && !vt.TenVaiTro.ToLower().Contains("admin"));
            }

            var listVaiTro = await query.ToListAsync();
            ViewBag.DanhSachVaiTro = new SelectList(listVaiTro, "MaVaiTro", "TenVaiTro", selectedMaVaiTro);
        }

        // ==========================================
        // 1. INDEX: Danh sách nhân viên
        // ==========================================
        public async Task<IActionResult> Index()
        {
            if (!CoQuyen("WEB_QL_NHANVIEN") && !CoQuyen("WEB_QL_NV_XEM"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();

            var query = _context.NhanViens
                .AsNoTracking()
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.SieuThi)
                .AsQueryable();

            if (!string.IsNullOrEmpty(maSTCuaToi))
            {
                query = query.Where(nv => nv.MaST == maSTCuaToi);
            }

            var danhSachNV = await query.OrderBy(nv => nv.MaNV).ToListAsync();

            ViewBag.MaSTCuaToi = maSTCuaToi;
            ViewBag.TenSTCuaToi = !string.IsNullOrEmpty(maSTCuaToi)
                ? (await _context.SieuThis.Where(st => st.MaST == maSTCuaToi).Select(st => st.TenST).FirstOrDefaultAsync())
                : null;

            return View(danhSachNV);
        }

        // ==========================================
        // 2. CREATE (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!CoQuyen("WEB_QL_NV_THEM"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            await LoadSieuThiBagAsync(maSTCuaToi);
            await LoadVaiTroBagAsync();

            return View(new NhanVien { MaST = maSTCuaToi ?? string.Empty, TrangThai = 1 });
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            if (!CoQuyen("WEB_QL_NV_THEM"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var vaiTro = await _context.VaiTros.FindAsync(nhanVien.MaVaiTro);

            if (vaiTro != null && (vaiTro.TenVaiTro.ToLower().Contains("quản lý hệ thống") || vaiTro.TenVaiTro.ToLower().Contains("admin")))
            {
                nhanVien.MaST = null;
            }
            else if (!string.IsNullOrEmpty(maSTCuaToi))
            {
                nhanVien.MaST = maSTCuaToi;
            }

            if (!string.IsNullOrEmpty(maSTCuaToi) || (vaiTro != null && (vaiTro.TenVaiTro.ToLower().Contains("quản lý hệ thống") || vaiTro.TenVaiTro.ToLower().Contains("admin"))))
            {
                ModelState.Remove(nameof(NhanVien.MaST));
            }

            if (await _context.NhanViens.AnyAsync(nv => nv.MaNV == nhanVien.MaNV))
            {
                ModelState.AddModelError("MaNV", "Mã nhân viên này đã tồn tại trong hệ thống.");
            }
            if (await _context.NhanViens.AnyAsync(nv => nv.TenDN == nhanVien.TenDN))
            {
                ModelState.AddModelError("TenDN", "Tên đăng nhập này đã được sử dụng.");
            }
            if (string.IsNullOrWhiteSpace(nhanVien.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu.");
            }

            if (ModelState.IsValid)
            {
                nhanVien.MatKhau = _passwordHasher.HashPassword(nhanVien, nhanVien.MatKhau!);
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadSieuThiBagAsync(nhanVien.MaST);
            await LoadVaiTroBagAsync(nhanVien.MaVaiTro);
            return View(nhanVien);
        }

        // ==========================================
        // 3. EDIT (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!CoQuyen("WEB_QL_NV_XEM") && !CoQuyen("WEB_QL_NV_SUA"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            var nhanVien = await _context.NhanViens.Include(n => n.VaiTro).FirstOrDefaultAsync(n => n.MaNV == id);
            if (nhanVien == null) return NotFound();

            if (!string.IsNullOrEmpty(maSTCuaToi) && nhanVien.MaST != maSTCuaToi)
            {
                return ChanDirectLink();
            }

            nhanVien.MatKhau = string.Empty;

            await LoadSieuThiBagAsync(nhanVien.MaST);
            await LoadVaiTroBagAsync(nhanVien.MaVaiTro);
            return View(nhanVien);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVien nhanVien)
        {
            if (!CoQuyen("WEB_QL_NV_SUA"))
            {
                return ChanDirectLink();
            }

            if (id != nhanVien.MaNV) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            var nhanVienGoc = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaNV == id);
            if (nhanVienGoc == null) return NotFound();

            if (!string.IsNullOrEmpty(maSTCuaToi) && nhanVienGoc.MaST != maSTCuaToi)
            {
                return ChanDirectLink();
            }

            var vaiTro = await _context.VaiTros.FindAsync(nhanVien.MaVaiTro);

            nhanVienGoc.HoTen = nhanVien.HoTen;
            nhanVienGoc.SoDT = nhanVien.SoDT;
            nhanVienGoc.TrangThai = nhanVien.TrangThai;
            nhanVienGoc.MaVaiTro = nhanVien.MaVaiTro;

            if (vaiTro != null && (vaiTro.TenVaiTro.ToLower().Contains("quản lý hệ thống") || vaiTro.TenVaiTro.ToLower().Contains("admin")))
            {
                nhanVienGoc.MaST = null;
            }
            else if (!string.IsNullOrEmpty(maSTCuaToi))
            {
                nhanVienGoc.MaST = maSTCuaToi;
            }
            else
            {
                nhanVienGoc.MaST = nhanVien.MaST;
            }

            if (!string.IsNullOrEmpty(maSTCuaToi) || (vaiTro != null && (vaiTro.TenVaiTro.ToLower().Contains("quản lý hệ thống") || vaiTro.TenVaiTro.ToLower().Contains("admin"))))
            {
                ModelState.Remove(nameof(NhanVien.MaST));
            }

            if (!string.IsNullOrWhiteSpace(nhanVien.MatKhau))
            {
                nhanVienGoc.MatKhau = _passwordHasher.HashPassword(nhanVienGoc, nhanVien.MatKhau);
            }

            ModelState.Clear();

            if (string.IsNullOrEmpty(nhanVienGoc.HoTen))
            {
                ModelState.AddModelError("HoTen", "Tên nhân viên không được để trống.");
            }
            if (string.IsNullOrEmpty(nhanVienGoc.MaVaiTro))
            {
                ModelState.AddModelError("MaVaiTro", "Vui lòng chọn vai trò nhân viên.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVienGoc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNV)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            nhanVienGoc.MatKhau = string.Empty;

            await LoadSieuThiBagAsync(nhanVienGoc.MaST);
            await LoadVaiTroBagAsync(nhanVienGoc.MaVaiTro);
            return View(nhanVienGoc);
        }

        // ==========================================
        // 4. DELETE (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_QL_NV_XOA"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var nhanVien = await _context.NhanViens.FindAsync(id);

            if (nhanVien != null)
            {
                if (!string.IsNullOrEmpty(maSTCuaToi) && nhanVien.MaST != maSTCuaToi)
                {
                    return ChanDirectLink();
                }

                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.MaNV == id);
        }
    }
}