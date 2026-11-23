using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly MiniMartDbContext _context;
        private readonly IPasswordHasher<NhanVien> _passwordHasher;

        public AccountController(MiniMartDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<NhanVien>();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Tìm nhân viên theo tên đăng nhập (Không bắt buộc TrangThai == 1 ở đây để kiểm tra trạng thái ngừng hoạt động)
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(nv => nv.TenDN == model.TenDN);

            bool matKhauHopLe = false;
            if (nhanVien != null && !string.IsNullOrEmpty(nhanVien.MatKhau))
            {
                var ketQua = _passwordHasher.VerifyHashedPassword(nhanVien, nhanVien.MatKhau, model.MatKhau);
                matKhauHopLe = ketQua != PasswordVerificationResult.Failed;
            }

            if (nhanVien != null && matKhauHopLe)
            {
                // KIỂM TRA 1: Nếu tài khoản ngừng hoạt động (TrangThai != 1)
                if (nhanVien.TrangThai != 1)
                {
                    TempData["ShowLoginErrorModal"] = true;
                    return View(model);
                }

                // Lấy danh sách MaPQ từ bảng trung gian VAITRO_PHANQUYEN
                var permissions = await _context.Database
                    .SqlQueryRaw<string>("SELECT \"MaPQ\" FROM \"VAITRO_PHANQUYEN\" WHERE \"MaVaiTro\" = {0}", nhanVien.MaVaiTro ?? "")
                    .ToListAsync();

                // KIỂM TRA 2: Nếu tài khoản chưa có quyền gốc "WEB" (Backoffice)
                if (!permissions.Contains("WEB"))
                {
                    TempData["ShowLoginErrorModal"] = true;
                    return View(model);
                }

                // Lưu danh sách quyền vào Session
                HttpContext.Session.SetObjectAsJson("UserPermissions", permissions);

                // Lấy tên siêu thị & vai trò hiển thị lên header
                string tenST = "Tất cả siêu thị";
                if (!string.IsNullOrEmpty(nhanVien.MaST))
                {
                    var sieuThi = await _context.SieuThis.FindAsync(nhanVien.MaST);
                    if (sieuThi != null) tenST = sieuThi.TenST;
                }

                string tenVaiTro = "Chưa phân vai trò";
                if (!string.IsNullOrEmpty(nhanVien.MaVaiTro))
                {
                    var vaiTro = await _context.VaiTros.FindAsync(nhanVien.MaVaiTro);
                    if (vaiTro != null) tenVaiTro = vaiTro.TenVaiTro;
                }

                // Lưu thông tin cơ bản vào Session
                HttpContext.Session.SetString("MaNV", nhanVien.MaNV);
                HttpContext.Session.SetString("HoTen", nhanVien.HoTen);
                HttpContext.Session.SetString("TenDN", nhanVien.TenDN);
                HttpContext.Session.SetString("ChucVu", tenVaiTro);
                HttpContext.Session.SetString("TenST", tenST);
                HttpContext.Session.SetString("MaST", string.IsNullOrEmpty(nhanVien.MaST) ? "ALL" : nhanVien.MaST);

                // ĐIỀU HƯỚNG THEO QUYỀN TỔNG QUAN (WEB_TONGQUAN)
                if (permissions.Contains("WEB_TONGQUAN"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Profile", "Account");
                }
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var maNV = HttpContext.Session.GetString("MaNV");
            if (string.IsNullOrEmpty(maNV)) return RedirectToAction("Login");

            var nhanVien = await _context.NhanViens
                .AsNoTracking()
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.SieuThi)
                .FirstOrDefaultAsync(nv => nv.MaNV == maNV);

            if (nhanVien == null) return RedirectToAction("Login");

            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(NhanVien input, string? matKhauHienTai, string? xacNhanMatKhau)
        {
            var maNV = HttpContext.Session.GetString("MaNV");
            if (string.IsNullOrEmpty(maNV)) return RedirectToAction("Login");

            var nhanVien = await _context.NhanViens
                .Include(nv => nv.VaiTro)
                .Include(nv => nv.SieuThi)
                .FirstOrDefaultAsync(nv => nv.MaNV == maNV);

            if (nhanVien == null) return RedirectToAction("Login");

            ModelState.Clear();
            var soDT = input.SoDT?.Trim();
            if (!string.IsNullOrEmpty(soDT) && (soDT.Length > 10 || !soDT.All(char.IsDigit)))
            {
                ModelState.AddModelError("SoDT", "Số điện thoại chỉ gồm tối đa 10 chữ số.");
            }

            var matKhauMoi = input.MatKhau;
            bool doiMatKhau = !string.IsNullOrEmpty(matKhauHienTai)
                           || !string.IsNullOrEmpty(matKhauMoi)
                           || !string.IsNullOrEmpty(xacNhanMatKhau);

            if (doiMatKhau)
            {
                if (string.IsNullOrEmpty(matKhauHienTai))
                    ModelState.AddModelError("matKhauHienTai", "Vui lòng nhập mật khẩu hiện tại.");

                if (string.IsNullOrEmpty(matKhauMoi))
                    ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu mới.");
                else if (matKhauMoi.Length < 6)
                    ModelState.AddModelError("MatKhau", "Mật khẩu mới phải có ít nhất 6 ký tự.");

                if (string.IsNullOrEmpty(xacNhanMatKhau))
                    ModelState.AddModelError("xacNhanMatKhau", "Vui lòng nhập lại mật khẩu mới.");
                else if (!string.IsNullOrEmpty(matKhauMoi) && matKhauMoi != xacNhanMatKhau)
                    ModelState.AddModelError("xacNhanMatKhau", "Xác nhận mật khẩu không khớp.");

                if (ModelState.IsValid)
                {
                    var ketQua = _passwordHasher.VerifyHashedPassword(nhanVien, nhanVien.MatKhau ?? "", matKhauHienTai!);
                    if (ketQua == PasswordVerificationResult.Failed)
                        ModelState.AddModelError("matKhauHienTai", "Mật khẩu hiện tại không đúng.");
                    else if (matKhauHienTai == matKhauMoi)
                        ModelState.AddModelError("MatKhau", "Mật khẩu mới phải khác mật khẩu hiện tại.");
                }
            }

            if (ModelState.IsValid)
            {
                nhanVien.SoDT = string.IsNullOrEmpty(soDT) ? null : soDT;
                if (doiMatKhau)
                {
                    nhanVien.MatKhau = _passwordHasher.HashPassword(nhanVien, matKhauMoi!);
                }

                await _context.SaveChangesAsync();
                TempData["ThongBao"] = doiMatKhau ? "Cập nhật thông tin và đổi mật khẩu thành công." : "Cập nhật thông tin thành công.";
                return RedirectToAction(nameof(Profile));
            }

            ViewBag.SoDTGoc = nhanVien.SoDT;
            nhanVien.SoDT = input.SoDT;
            return View(nhanVien);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}