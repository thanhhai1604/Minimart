using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("CHITIETPHIEUNHAP")]
    public class ChiTietPhieuNhap
    {
        [Column("MaPN")]
        [Required]
        [StringLength(30)]
        public string MaPN { get; set; } = string.Empty;

        [Column("MaSP")]
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        [StringLength(20)]
        [Display(Name = "Mã sản phẩm")]
        public string MaSP { get; set; } = string.Empty;

        [Column("SoLuongNhap")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
        [Display(Name = "Số lượng nhập")]
        public int SoLuongNhap { get; set; }

        [Column("DonGiaNhap")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá nhập không được nhỏ hơn 0.")]
        [Display(Name = "Đơn giá nhập")]
        public decimal DonGiaNhap { get; set; }

        [ForeignKey("MaPN")]
        public virtual PhieuNhap? PhieuNhapNavigation { get; set; }

        [ForeignKey("MaSP")]
        public virtual SanPham? SanPhamNavigation { get; set; }
    }
}