using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Data;
using MiniMart.Web.Helpers;
using MiniMart.Web.Models;

namespace MiniMart.Web.Controllers
{
    public class VaiTroController : Controller
    {
        private readonly MiniMartDbContext _context;

        public VaiTroController(MiniMartDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách vai trò (Index)
        public async Task<IActionResult> Index()
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VAITRO") && !PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_XEM"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            var danhSachVaiTro = await _context.VaiTros.ToListAsync();
            return View(danhSachVaiTro);
        }

        // 2. Thêm mới vai trò (GET) - Chặn Direct Link
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_THEM"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            var tatCaQuyen = await _context.PhanQuyens.ToListAsync();
            var model = new VaiTroDetails
            {
                DanhSachQuyen = XayDungCayPhanQuyen(tatCaQuyen, new List<string>())
            };

            return View(model);
        }

        // 2. Thêm mới vai trò (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VaiTroDetails model)
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_THEM"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            if (await _context.VaiTros.AnyAsync(v => v.MaVaiTro == model.MaVaiTro))
            {
                ModelState.AddModelError("MaVaiTro", "Mã vai trò này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                var vaiTro = new VaiTro
                {
                    MaVaiTro = model.MaVaiTro,
                    TenVaiTro = model.TenVaiTro,
                    MoTa = model.MoTa
                };
                _context.VaiTros.Add(vaiTro);

                if (model.SelectedQuyenIds != null && model.SelectedQuyenIds.Any())
                {
                    foreach (var maPQ in model.SelectedQuyenIds)
                    {
                        var sql = "INSERT INTO \"VAITRO_PHANQUYEN\" (\"MaVaiTro\", \"MaPQ\") VALUES (@p0, @p1)";
                        await _context.Database.ExecuteSqlRawAsync(sql, vaiTro.MaVaiTro, maPQ);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tatCaQuyen = await _context.PhanQuyens.ToListAsync();
            model.DanhSachQuyen = XayDungCayPhanQuyen(tatCaQuyen, model.SelectedQuyenIds);
            return View(model);
        }

        // 3. Cập nhật vai trò (GET) - Chặn Direct Link
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_XEM"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro == null) return NotFound();

            var nhanViens = await LayDanhSachNhanVien(id);

            var quyenDaChon = new List<string>();
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT \"MaPQ\" FROM \"VAITRO_PHANQUYEN\" WHERE \"MaVaiTro\" = @maVaiTro";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@maVaiTro";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                await _context.Database.OpenConnectionAsync();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        quyenDaChon.Add(result.GetString(0));
                    }
                }
                await _context.Database.CloseConnectionAsync();
            }

            var tatCaQuyen = await _context.PhanQuyens.ToListAsync();

            var model = new VaiTroDetails
            {
                MaVaiTro = vaiTro.MaVaiTro,
                TenVaiTro = vaiTro.TenVaiTro,
                MoTa = vaiTro.MoTa,
                DanhSachNhanVien = nhanViens,
                SelectedQuyenIds = quyenDaChon,
                DanhSachQuyen = XayDungCayPhanQuyen(tatCaQuyen, quyenDaChon)
            };

            return View(model);
        }

        // 4. Cập nhật vai trò (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VaiTroDetails model)
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_SUA"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            if (id != model.MaVaiTro) return NotFound();

            var vaiTroGoc = await _context.VaiTros.FindAsync(id);
            if (vaiTroGoc == null) return NotFound();

            if (ModelState.IsValid)
            {
                vaiTroGoc.TenVaiTro = model.TenVaiTro;
                vaiTroGoc.MoTa = model.MoTa;
                _context.Update(vaiTroGoc);

                var deleteSql = "DELETE FROM \"VAITRO_PHANQUYEN\" WHERE \"MaVaiTro\" = @p0";
                await _context.Database.ExecuteSqlRawAsync(deleteSql, id);

                if (model.SelectedQuyenIds != null && model.SelectedQuyenIds.Any())
                {
                    foreach (var maPQ in model.SelectedQuyenIds)
                    {
                        var insertSql = "INSERT INTO \"VAITRO_PHANQUYEN\" (\"MaVaiTro\", \"MaPQ\") VALUES (@p0, @p1)";
                        await _context.Database.ExecuteSqlRawAsync(insertSql, id, maPQ);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.DanhSachNhanVien = await LayDanhSachNhanVien(id);
            var tatCaQuyen = await _context.PhanQuyens.ToListAsync();
            model.DanhSachQuyen = XayDungCayPhanQuyen(tatCaQuyen, model.SelectedQuyenIds);
            return View(model);
        }

        // 5. Xóa vai trò (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!PermissionHelper.HasPermission(HttpContext, "WEB_HT_VT_XOA"))
            {
                HttpContext.Session.SetString("ShowDirectLinkError", "true");
                return RedirectToAction("Profile", "Account");
            }

            var vaiTro = await _context.VaiTros.FindAsync(id);
            if (vaiTro != null)
            {
                var deleteQuyenSql = "DELETE FROM \"VAITRO_PHANQUYEN\" WHERE \"MaVaiTro\" = @p0";
                await _context.Database.ExecuteSqlRawAsync(deleteQuyenSql, id);

                _context.VaiTros.Remove(vaiTro);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<NhanVienThuocVaiTro>> LayDanhSachNhanVien(string maVaiTro)
        {
            return await _context.NhanViens
                .Where(nv => nv.MaVaiTro == maVaiTro)
                .Select(nv => new NhanVienThuocVaiTro
                {
                    MaNV = nv.MaNV,
                    HoTen = nv.HoTen,
                    TenDN = nv.TenDN,
                    SoDT = nv.SoDT ?? "",
                    TenST = nv.SieuThi != null ? nv.SieuThi.TenST : null,
                    TrangThai = nv.TrangThai
                })
                .ToListAsync();
        }

        private List<PhanQuyenCheckbox> XayDungCayPhanQuyen(List<PhanQuyen> tatCaQuyen, List<string> quyenDaChon)
        {
            var dic = tatCaQuyen.Select(q => new PhanQuyenCheckbox
            {
                MaPQ = q.MaPQ,
                TenPQ = q.TenPQ,
                NenTang = q.NenTang,
                QuyenCha = q.QuyenCha,
                MoTa = q.MoTa,
                DaChon = quyenDaChon.Contains(q.MaPQ),
                Children = new List<PhanQuyenCheckbox>()
            }).ToDictionary(q => q.MaPQ);

            var cayQuyen = new List<PhanQuyenCheckbox>();

            foreach (var item in dic.Values)
            {
                if (string.IsNullOrEmpty(item.QuyenCha) || !dic.ContainsKey(item.QuyenCha))
                {
                    cayQuyen.Add(item);
                }
                else
                {
                    dic[item.QuyenCha].Children.Add(item);
                }
            }

            return cayQuyen;
        }
    }
}