using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("HOADON")]
    public class HoaDon
    {
        [Key]
        [Column("MaHD")]
        [Required(ErrorMessage = "Mã hóa đơn không được để trống.")]
        [StringLength(20)]
        [Display(Name = "Mã hóa đơn")]
        public string MaHD { get; set; } = string.Empty;

        [Column("NgayLap")]
        [Display(Name = "Thời gian mua")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column("TongTienTruocThue", TypeName = "decimal(18,2)")]
        [Display(Name = "Tiền trước thuế")]
        public decimal TongTienTruocThue { get; set; } = 0;

        [Column("TongTienThue", TypeName = "decimal(18,2)")]
        [Display(Name = "Tiền thuế")]
        public decimal TongTienThue { get; set; } = 0;

        [Column("TongTien", TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; } = 0;

        [Column("DiaChiGiaoHang")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string? DiaChiGiaoHang { get; set; }

        [Column("GhiChu")]
        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Column("MaNV")]
        [StringLength(20)]
        [Display(Name = "Mã nhân viên")]
        public string? MaNV { get; set; }

        [Column("MaKH")]
        [StringLength(20)]
        [Display(Name = "Mã khách hàng")]
        public string? MaKH { get; set; }

        [Column("MaST")]
        [StringLength(20)]
        [Display(Name = "Mã siêu thị")]
        public string? MaST { get; set; }

        // Navigation properties liên kết bảng Khách hàng
        [ForeignKey("MaKH")]
        public virtual KhachHang? KhachHangNavigation { get; set; }

        // Navigation properties liên kết bảng Siêu thị
        [ForeignKey("MaST")]
        public virtual SieuThi? SieuThiNavigation { get; set; }
    }
}