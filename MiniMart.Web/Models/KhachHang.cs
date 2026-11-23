using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("KHACHHANG")]
    public class KhachHang
    {
        [Key]
        [Column("MaKH")]
        [Required(ErrorMessage = "Mã khách hàng không được để trống.")]
        [StringLength(20)]
        [Display(Name = "Mã khách hàng")]
        public string MaKH { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        [Column("TenKH")]
        [StringLength(150)]
        [Display(Name = "Họ tên khách hàng")]
        public string TenKH { get; set; } = string.Empty;

        [Column("SoDT")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDT { get; set; }

        [Column("Email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Column("DiaChi")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Column("DiemTichLuy")]
        [Display(Name = "Điểm tích lũy")]
        public decimal DiemTichLuy { get; set; } = 0;

        [Column("DiemSuDung")]
        [Display(Name = "Điểm sử dụng")]
        public decimal DiemSuDung { get; set; } = 0;

        [Column("DiemKhaDung")]
        [Display(Name = "Điểm khả dụng")]
        public decimal DiemKhaDung { get; set; } = 0;

        // Navigation property liên kết danh sách hóa đơn (HOADON)
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}