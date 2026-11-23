using Microsoft.EntityFrameworkCore;
using MiniMart.Web.Models;

namespace MiniMart.Web.Data
{
    // Áp dụng Primary Constructor của C# 12 trực tiếp trên tên Class
    public class MiniMartDbContext(DbContextOptions<MiniMartDbContext> options) : DbContext(options)
    {
        public DbSet<SieuThi> SieuThis { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<PhanQuyen> PhanQuyens { get; set; }
        public DbSet<VaiTroPhanQuyen> VaiTroPhanQuyens { get; set; }
        public DbSet<POS> POS { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<CaBanHang> CaBanHangs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<Thue> Thues { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<PTThanhToan> PTThanhToans { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<SanPhamTonKho> SanPhamTonKhos { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<ChiTietThanhToan> ChiTietThanhToans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính phức hợp cho các bảng có khóa kép hoặc bảng trung gian (POS dùng khóa kép MaPOS + MaST)
            modelBuilder.Entity<VaiTroPhanQuyen>().HasKey(t => new { t.MaVaiTro, t.MaPQ });
            modelBuilder.Entity<SanPhamTonKho>().HasKey(t => new { t.MaST, t.MaSP });
            modelBuilder.Entity<ChiTietPhieuNhap>().HasKey(t => new { t.MaPN, t.MaSP });
            modelBuilder.Entity<ChiTietHoaDon>().HasKey(t => new { t.MaHD, t.MaSP });
            modelBuilder.Entity<ChiTietThanhToan>().HasKey(t => new { t.MaHD, t.MaPTTT });
            modelBuilder.Entity<POS>().HasKey(t => new { t.MaPOS, t.MaST });

            // LƯU Ý: KHÔNG khai báo .HasPrincipalKey(p => p.MaPOS) cho CaBanHang -> POS.
            // Lệnh đó tạo "khóa phụ" duy nhất trên riêng MaPOS, trong khi MaPOS bị trùng giữa các siêu thị
            // (POS01 của MNM001 và POS01 của MNM002), khiến EF gộp 2 dòng thành 1 và hiển thị sai.
            // Khóa ngoại đã có sẵn trong database nên không cần khai báo lại ở đây.
            // Nếu bảng CABANHANG có cột MaST, muốn khai báo thì dùng khóa kép đúng thứ tự khóa chính của POS:
            //   modelBuilder.Entity<CaBanHang>()
            //       .HasOne<POS>().WithMany()
            //       .HasForeignKey(c => new { c.MaPOS, c.MaST })
            //       .HasPrincipalKey(p => new { p.MaPOS, p.MaST });
        }
    }
}