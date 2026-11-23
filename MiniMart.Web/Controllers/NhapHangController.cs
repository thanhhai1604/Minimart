using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class ChiTietPhieuNhapInputDto
    {
        public string MaSP { get; set; } = string.Empty;
        public int SoLuongNhap { get; set; }
        public decimal DonGiaNhap { get; set; }
    }

    public partial class NhapHangController(MiniMartDbContext context) : Controller
    {
        private readonly MiniMartDbContext _context = context;

        private static readonly JsonSerializerOptions ChiTietJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [GeneratedRegex(@"^PN\d{8}-(\d{1,5})$")]
        private static partial Regex MaPhieuDigitsRegex();

        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigitsRegex();

        [GeneratedRegex(@"^PN\d{8}-\d{5}$")]
        private static partial Regex MaPhieuChuanRegex();

        private bool CoQuyen(string maPQ) => PermissionHelper.HasPermission(HttpContext, maPQ);

        private RedirectToActionResult ChanDirectLink()
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

        private string LayMaNVCuaToi()
        {
            return HttpContext.Session.GetString("MaNV")
                ?? HttpContext.Session.GetString("TenDN")
                ?? string.Empty;
        }

        private static DateTime ChuanHoaNgayNhapUtcTuLocal(DateTime ngayNhap)
        {
            var target = ngayNhap == default ? DateTime.Now : ngayNhap;
            if (target.Kind == DateTimeKind.Utc)
            {
                return target;
            }

            var unspecified = DateTime.SpecifyKind(target, DateTimeKind.Unspecified);
            var offset = TimeZoneInfo.Local.GetUtcOffset(unspecified);
            return new DateTimeOffset(unspecified, offset).UtcDateTime;
        }

        private static DateTime ChuyenUtcSangLocalHienThi(DateTime ngayNhapDb)
        {
            if (ngayNhapDb == default)
            {
                return DateTime.Now;
            }

            if (ngayNhapDb.Kind == DateTimeKind.Local)
            {
                return ngayNhapDb;
            }

            var utc = DateTime.SpecifyKind(ngayNhapDb, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
        }

        private async Task<string?> ChuanHoaMaNVHopLeAsync(string? maNV)
        {
            var candidate = string.IsNullOrWhiteSpace(maNV) ? LayMaNVCuaToi() : maNV.Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return null;
            }

            var nv = await _context.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNV == candidate || x.TenDN == candidate);

            return nv?.MaNV;
        }

        private async Task<string> LayTenNhanVienAsync(string? maNV)
        {
            var hoTenSession = HttpContext.Session.GetString("HoTen");
            if (!string.IsNullOrWhiteSpace(hoTenSession))
            {
                return hoTenSession.Trim();
            }

            if (!string.IsNullOrWhiteSpace(maNV))
            {
                var nv = await _context.NhanViens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaNV == maNV || x.TenDN == maNV);
                if (nv != null && !string.IsNullOrWhiteSpace(nv.HoTen))
                {
                    return nv.HoTen.Trim();
                }
            }

            return string.Empty;
        }

        private async Task<string> LaySanPhamTonKhoJsonBySieuThiAsync(string? maST)
        {
            if (string.IsNullOrWhiteSpace(maST))
            {
                return "[]";
            }

            var targetMaST = maST.Trim();
            var dsSP = await _context.SanPhamTonKhos
                .AsNoTracking()
                .Where(tk => tk.MaST == targetMaST)
                .Include(tk => tk.SanPhamNavigation)
                .OrderBy(tk => tk.SanPhamNavigation != null ? tk.SanPhamNavigation.TenSP : tk.MaSP)
                .Select(tk => new
                {
                    tk.MaSP,
                    TenSP = tk.SanPhamNavigation != null ? tk.SanPhamNavigation.TenSP : tk.MaSP,
                    GiaNhap = Convert.ToInt64(Math.Round(tk.GiaNhap, 0, MidpointRounding.AwayFromZero))
                })
                .ToListAsync();

            return JsonSerializer.Serialize(dsSP);
        }

        private async Task LoadFormLookupsAsync(string? selectedMaST = null, string? selectedMaNCC = null, string? maNV = null)
        {
            var maSTCuaToi = LayMaSTCuaToi();
            var sieuThiQuery = _context.SieuThis.AsNoTracking().OrderBy(s => s.TenST);
            var dsSieuThi = maSTCuaToi != null
                ? await sieuThiQuery.Where(s => s.MaST == maSTCuaToi).ToListAsync()
                : await sieuThiQuery.ToListAsync();

            var dsNCC = await _context.NhaCungCaps
                .AsNoTracking()
                .OrderBy(n => n.TenNCC)
                .ToListAsync();

            var effectiveMaST = maSTCuaToi ?? (string.IsNullOrWhiteSpace(selectedMaST) ? null : selectedMaST.Trim());

            ViewBag.DanhSachSieuThi = new SelectList(dsSieuThi, "MaST", "TenST", effectiveMaST);
            ViewBag.DanhSachNhaCungCap = new SelectList(dsNCC, "MaNCC", "TenNCC", selectedMaNCC);
            ViewBag.TenNhanVienLap = await LayTenNhanVienAsync(maNV);
            ViewBag.DanhSachSanPhamJson = await LaySanPhamTonKhoJsonBySieuThiAsync(effectiveMaST);
        }

        private static string ChuanHoaMaPhieuNhap(string? rawInput, DateTime ngayNhap)
        {
            var trimmed = (rawInput ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            var match = MaPhieuDigitsRegex().Match(trimmed);
            string digits;
            if (match.Success)
            {
                digits = match.Groups[1].Value;
            }
            else
            {
                digits = NonDigitsRegex().Replace(trimmed, string.Empty);
                if (digits.Length > 5)
                {
                    digits = digits[..5];
                }
            }

            if (string.IsNullOrEmpty(digits) || !int.TryParse(digits, out int soPhieu) || soPhieu <= 0)
            {
                return string.Empty;
            }

            return $"PN{ngayNhap:ddMMyyyy}-{soPhieu:D5}";
        }

        private static List<ChiTietPhieuNhapInputDto> ParseChiTietJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];

            try
            {
                var raw = JsonSerializer.Deserialize<List<ChiTietPhieuNhapInputDto>>(json, ChiTietJsonOptions) ?? [];

                return [.. raw
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaSP) && x.SoLuongNhap > 0 && x.DonGiaNhap >= 0)
                    .GroupBy(x => x.MaSP.Trim())
                    .Select(g => new ChiTietPhieuNhapInputDto
                    {
                        MaSP = g.Key,
                        SoLuongNhap = g.Sum(i => i.SoLuongNhap),
                        DonGiaNhap = Math.Round(g.Last().DonGiaNhap, 2, MidpointRounding.AwayFromZero)
                    })];
            }
            catch
            {
                return [];
            }
        }

        private async Task CongTonKhoKhiGiaoHangAsync(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> chiTiets)
        {
            if (phieuNhap.TrangThai != 1)
            {
                return;
            }

            foreach (var ct in chiTiets)
            {
                decimal giaNhapMoi = ct.DonGiaNhap;
                decimal giaGocMoi = Math.Round(giaNhapMoi * 1.2m, 0, MidpointRounding.AwayFromZero);

                var tonKho = await _context.SanPhamTonKhos
                    .FirstOrDefaultAsync(t => t.MaST == phieuNhap.MaST && t.MaSP == ct.MaSP);

                if (tonKho != null)
                {
                    tonKho.SoLuongTon += ct.SoLuongNhap;
                    tonKho.GiaNhap = giaNhapMoi;
                    tonKho.GiaGoc = giaGocMoi;
                    if (tonKho.GiaBan <= 0)
                    {
                        tonKho.GiaBan = giaGocMoi;
                    }
                }
                else
                {
                    await _context.SanPhamTonKhos.AddAsync(new SanPhamTonKho
                    {
                        MaST = phieuNhap.MaST,
                        MaSP = ct.MaSP,
                        GiaNhap = giaNhapMoi,
                        GiaGoc = giaGocMoi,
                        GiaBan = giaGocMoi,
                        SoLuongTon = ct.SoLuongNhap,
                        TrangThaiKinhDoanh = 1
                    });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> LaySanPhamTheoSieuThi(string maST)
        {
            if (!CoQuyen("WEB_HH_NH_THEM") && !CoQuyen("WEB_HH_NH_CAPNHAT") && !CoQuyen("WEB_HH_NH_XEM"))
            {
                return Unauthorized();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var targetMaST = maSTCuaToi ?? maST?.Trim();
            if (string.IsNullOrWhiteSpace(targetMaST))
            {
                return Content("[]", "application/json");
            }

            var json = await LaySanPhamTonKhoJsonBySieuThiAsync(targetMaST);
            return Content(json, "application/json");
        }

        public async Task<IActionResult> Index(string? maST = null)
        {
            if (!CoQuyen("WEB_HH_NHAPHANG") && !CoQuyen("WEB_HH_NH_XEM"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var filterMaST = maSTCuaToi ?? (string.IsNullOrWhiteSpace(maST) ? null : maST.Trim());

            ViewBag.GioiHanSieuThi = maSTCuaToi != null;
            ViewBag.SelectedMaST = filterMaST;
            ViewBag.DanhSachSieuThi = await _context.SieuThis
                .AsNoTracking()
                .OrderBy(s => s.TenST)
                .ToListAsync();

            var query = _context.PhieuNhaps
                .AsNoTracking()
                .Include(p => p.NhaCungCapNavigation)
                .Include(p => p.SieuThiNavigation)
                .Include(p => p.NhanVienNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterMaST))
            {
                query = query.Where(p => p.MaST == filterMaST);
            }

            var danhSach = await query
                .OrderByDescending(p => p.NgayNhap)
                .ThenByDescending(p => p.MaPN)
                .ToListAsync();

            foreach (var item in danhSach)
            {
                item.NgayNhap = ChuyenUtcSangLocalHienThi(item.NgayNhap);
            }

            return View(danhSach);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!CoQuyen("WEB_HH_NH_THEM"))
            {
                return ChanDirectLink();
            }

            var maSTCuaToi = LayMaSTCuaToi();
            var maNV = LayMaNVCuaToi();

            ViewBag.GioiHanSieuThi = maSTCuaToi != null;
            ViewBag.ChiTietJson = "[]";
            await LoadFormLookupsAsync(maSTCuaToi, null, maNV);

            var model = new PhieuNhap
            {
                NgayNhap = DateTime.Now,
                MaST = maSTCuaToi ?? string.Empty,
                MaNV = maNV,
                TrangThai = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuNhap model, string? chiTietJson)
        {
            if (!CoQuyen("WEB_HH_NH_THEM"))
            {
                return ChanDirectLink();
            }

            ModelState.Remove(nameof(PhieuNhap.NhaCungCapNavigation));
            ModelState.Remove(nameof(PhieuNhap.SieuThiNavigation));
            ModelState.Remove(nameof(PhieuNhap.NhanVienNavigation));
            ModelState.Remove(nameof(PhieuNhap.ChiTietPhieuNhaps));

            var maSTCuaToi = LayMaSTCuaToi();
            model.MaST = maSTCuaToi ?? (model.MaST?.Trim() ?? string.Empty);
            model.MaNV = await ChuanHoaMaNVHopLeAsync(model.MaNV);
            model.MaNCC = model.MaNCC?.Trim() ?? string.Empty;
            model.TrangThai = model.TrangThai == 1 ? 1 : 0;

            var ngayNhapLocal = model.NgayNhap == default ? DateTime.Now : model.NgayNhap;
            model.NgayNhap = ChuanHoaNgayNhapUtcTuLocal(ngayNhapLocal);

            var maPNChuanHoa = ChuanHoaMaPhieuNhap(model.MaPN, ngayNhapLocal);
            if (string.IsNullOrWhiteSpace(maPNChuanHoa) || !MaPhieuChuanRegex().IsMatch(maPNChuanHoa))
            {
                ModelState.AddModelError(nameof(PhieuNhap.MaPN), "Vui lòng nhập số phiếu hợp lệ từ 1 đến 99999.");
            }
            else
            {
                model.MaPN = maPNChuanHoa;
                if (await _context.PhieuNhaps.AnyAsync(p => p.MaPN == model.MaPN))
                {
                    ModelState.AddModelError(nameof(PhieuNhap.MaPN), $"Mã phiếu nhập '{model.MaPN}' đã tồn tại trong hệ thống.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.MaST) && ModelState.GetFieldValidationState(nameof(PhieuNhap.MaST)) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
            {
                ModelState.AddModelError(nameof(PhieuNhap.MaST), "Vui lòng chọn siêu thị nhập hàng.");
            }

            if (string.IsNullOrWhiteSpace(model.MaNCC) && ModelState.GetFieldValidationState(nameof(PhieuNhap.MaNCC)) == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
            {
                ModelState.AddModelError(nameof(PhieuNhap.MaNCC), "Vui lòng chọn nhà cung cấp.");
            }

            var chiTietDtos = ParseChiTietJson(chiTietJson);
            if (chiTietDtos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng thêm ít nhất một sản phẩm hợp lệ vào phiếu nhập.");
            }

            if (!ModelState.IsValid)
            {
                model.NgayNhap = ngayNhapLocal;
                ViewBag.GioiHanSieuThi = maSTCuaToi != null;
                ViewBag.ChiTietJson = JsonSerializer.Serialize(chiTietDtos);
                await LoadFormLookupsAsync(model.MaST, model.MaNCC, model.MaNV);
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.PhieuNhaps.Add(model);

                var chiTietEntities = chiTietDtos.Select(d => new ChiTietPhieuNhap
                {
                    MaPN = model.MaPN,
                    MaSP = d.MaSP,
                    SoLuongNhap = d.SoLuongNhap,
                    DonGiaNhap = d.DonGiaNhap
                }).ToList();

                await _context.ChiTietPhieuNhaps.AddRangeAsync(chiTietEntities);

                if (model.TrangThai == 1)
                {
                    await CongTonKhoKhiGiaoHangAsync(model, chiTietEntities);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Tạo phiếu nhập hàng '{model.MaPN}' thành công!";
                return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;
                model.NgayNhap = ngayNhapLocal;
                ModelState.AddModelError(string.Empty, $"Không thể lưu phiếu nhập hàng: {innerMsg}");
                ViewBag.GioiHanSieuThi = maSTCuaToi != null;
                ViewBag.ChiTietJson = JsonSerializer.Serialize(chiTietDtos);
                await LoadFormLookupsAsync(model.MaST, model.MaNCC, model.MaNV);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!CoQuyen("WEB_HH_NH_XEM"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var maPN = id.Trim();
            var phieuNhap = await _context.PhieuNhaps
                .AsNoTracking()
                .Include(p => p.NhaCungCapNavigation)
                .Include(p => p.SieuThiNavigation)
                .Include(p => p.NhanVienNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.SanPhamNavigation)
                .FirstOrDefaultAsync(p => p.MaPN == maPN);

            if (phieuNhap == null) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && !string.Equals(phieuNhap.MaST, maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            phieuNhap.NgayNhap = ChuyenUtcSangLocalHienThi(phieuNhap.NgayNhap);

            var chiTietDtos = phieuNhap.ChiTietPhieuNhaps.Select(ct => new ChiTietPhieuNhapInputDto
            {
                MaSP = ct.MaSP,
                SoLuongNhap = ct.SoLuongNhap,
                DonGiaNhap = ct.DonGiaNhap
            }).ToList();

            ViewBag.GioiHanSieuThi = maSTCuaToi != null;
            ViewBag.ChiTietJson = JsonSerializer.Serialize(chiTietDtos);
            await LoadFormLookupsAsync(phieuNhap.MaST, phieuNhap.MaNCC, phieuNhap.MaNV);

            return View(phieuNhap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PhieuNhap model, string? chiTietJson)
        {
            if (!CoQuyen("WEB_HH_NH_CAPNHAT"))
            {
                return ChanDirectLink();
            }

            ModelState.Remove(nameof(PhieuNhap.NhaCungCapNavigation));
            ModelState.Remove(nameof(PhieuNhap.SieuThiNavigation));
            ModelState.Remove(nameof(PhieuNhap.NhanVienNavigation));
            ModelState.Remove(nameof(PhieuNhap.ChiTietPhieuNhaps));

            var targetMaPN = string.IsNullOrWhiteSpace(model.MaPN) ? id?.Trim() : model.MaPN.Trim();
            if (string.IsNullOrWhiteSpace(targetMaPN)) return NotFound();

            var existing = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.MaPN == targetMaPN);

            if (existing == null) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && !string.Equals(existing.MaST, maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            if (existing.TrangThai == 1)
            {
                TempData["ErrorMessage"] = $"Phiếu nhập '{existing.MaPN}' đã giao hàng nên không thể chỉnh sửa.";
                return RedirectToAction(nameof(Edit), new { id = existing.MaPN });
            }

            var chiTietDtos = ParseChiTietJson(chiTietJson);
            if (chiTietDtos.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Phiếu nhập phải có ít nhất một sản phẩm hợp lệ.");
            }

            var ngayNhapLocal = model.NgayNhap == default ? DateTime.Now : model.NgayNhap;

            if (!ModelState.IsValid)
            {
                model.MaPN = existing.MaPN;
                model.MaST = existing.MaST;
                model.MaNV = existing.MaNV;
                model.NgayNhap = ngayNhapLocal;
                ViewBag.GioiHanSieuThi = maSTCuaToi != null;
                ViewBag.ChiTietJson = JsonSerializer.Serialize(chiTietDtos);
                await LoadFormLookupsAsync(existing.MaST, model.MaNCC, existing.MaNV);
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int trangThaiMoi = model.TrangThai == 1 ? 1 : 0;

                existing.NgayNhap = ChuanHoaNgayNhapUtcTuLocal(ngayNhapLocal);
                existing.MaNCC = model.MaNCC.Trim();
                existing.TrangThai = trangThaiMoi;

                _context.ChiTietPhieuNhaps.RemoveRange(existing.ChiTietPhieuNhaps);

                var chiTietMoi = chiTietDtos.Select(d => new ChiTietPhieuNhap
                {
                    MaPN = existing.MaPN,
                    MaSP = d.MaSP,
                    SoLuongNhap = d.SoLuongNhap,
                    DonGiaNhap = d.DonGiaNhap
                }).ToList();

                await _context.ChiTietPhieuNhaps.AddRangeAsync(chiTietMoi);

                if (trangThaiMoi == 1)
                {
                    await CongTonKhoKhiGiaoHangAsync(existing, chiTietMoi);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Cập nhật phiếu nhập '{existing.MaPN}' thành công!";
                return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;
                existing.NgayNhap = ngayNhapLocal;
                ModelState.AddModelError(string.Empty, $"Không thể cập nhật phiếu nhập hàng: {innerMsg}");
                ViewBag.GioiHanSieuThi = maSTCuaToi != null;
                ViewBag.ChiTietJson = JsonSerializer.Serialize(chiTietDtos);
                await LoadFormLookupsAsync(existing.MaST, model.MaNCC, existing.MaNV);
                return View(existing);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!CoQuyen("WEB_HH_NH_HUY"))
            {
                return ChanDirectLink();
            }

            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var maPN = id.Trim();
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                .FirstOrDefaultAsync(p => p.MaPN == maPN);

            if (phieuNhap == null) return NotFound();

            var maSTCuaToi = LayMaSTCuaToi();
            if (maSTCuaToi != null && !string.Equals(phieuNhap.MaST, maSTCuaToi, StringComparison.OrdinalIgnoreCase))
            {
                return ChanDirectLink();
            }

            if (phieuNhap.TrangThai == 1)
            {
                TempData["ErrorMessage"] = $"Không thể hủy phiếu nhập '{phieuNhap.MaPN}' vì đã giao hàng.";
                return RedirectToAction(nameof(Edit), new { id = phieuNhap.MaPN });
            }

            try
            {
                if (phieuNhap.ChiTietPhieuNhaps.Count > 0)
                {
                    _context.ChiTietPhieuNhaps.RemoveRange(phieuNhap.ChiTietPhieuNhaps);
                }

                _context.PhieuNhaps.Remove(phieuNhap);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Hủy phiếu nhập hàng '{phieuNhap.MaPN}' thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["ForeignKeyDeleteError"] =
                    $"Không thể hủy phiếu nhập hàng \"{phieuNhap.MaPN}\" vì còn dữ liệu liên quan trong hệ thống.";
            }

            return RedirectToAction(nameof(Index), new { maST = maSTCuaToi });
        }
    }
}