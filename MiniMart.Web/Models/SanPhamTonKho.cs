using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("SANPHAMTONKHO")]
    public class SanPhamTonKho
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        [Display(Name = "Mã siêu thị")]
        public string MaST { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        [StringLength(20)]
        [Display(Name = "Mã sản phẩm")]
        public string MaSP { get; set; } = string.Empty;

        [Column("GiaNhap", TypeName = "numeric(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal GiaNhap { get; set; }

        [Column("GiaGoc", TypeName = "numeric(18,2)")]
        [Display(Name = "Giá gốc")]
        public decimal GiaGoc { get; set; }

        [Column("GiaBan", TypeName = "numeric(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal GiaBan { get; set; }

        [Column("SoLuongTon")]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; }

        [Column("TrangThaiKinhDoanh")]
        [Display(Name = "Trạng thái kinh doanh")]
        public int TrangThaiKinhDoanh { get; set; }

        [ForeignKey(nameof(MaST))]
        public virtual SieuThi? SieuThiNavigation { get; set; }

        [ForeignKey(nameof(MaSP))]
        public virtual SanPham? SanPhamNavigation { get; set; }
    }
}