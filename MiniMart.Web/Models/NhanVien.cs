using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("NHANVIEN")]
    public class NhanVien
    {
        [Key]
        [Column("MaNV")]
        [StringLength(20)]
        [Display(Name = "Mã nhân viên")]
        public string MaNV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhân viên không được để trống.")]
        [Column("HoTen")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [Column("TenDN")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDN { get; set; } = string.Empty;

        // Dùng ? cho Mật khẩu để khi bỏ trống ở trang Edit không bị lỗi validate bắt buộc cứng từ model
        [Column("MatKhau")]
        [StringLength(255)]
        [Display(Name = "Mật khẩu")]
        public string? MatKhau { get; set; }

        [Column("SoDT")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDT { get; set; }

        [Column("TrangThai")]
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1; // 1: Đang làm việc, 0: Ngừng làm việc

        [Required(ErrorMessage = "Vui lòng chọn vai trò cho nhân viên.")]
        [Column("MaVaiTro")]
        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string MaVaiTro { get; set; } = string.Empty;

        // Cho phép null đối với Quản lý hệ thống (quản lý toàn quốc)
        [Column("MaST")]
        [StringLength(50)]
        [Display(Name = "Siêu thị trực thuộc")]
        public string? MaST { get; set; }

        // ==========================================
        // CÁC THUỘC TÍNH NAVIGATION (DÙNG DẤU ?)
        // ==========================================

        [ForeignKey("MaVaiTro")]
        public virtual VaiTro? VaiTro { get; set; }

        [ForeignKey("MaST")]
        public virtual SieuThi? SieuThi { get; set; }
    }
}