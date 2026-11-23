using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("SANPHAM")]
    public class SanPham
    {
        [Key]
        [Column("MaSP")]
        [Required(ErrorMessage = "Mã sản phẩm không được để trống.")]
        [StringLength(20)]
        [Display(Name = "Mã sản phẩm")]
        public string MaSP { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã UPC không được để trống.")]
        [Column("UPC")]
        [StringLength(50)]
        [Display(Name = "Mã UPC")]
        public string UPC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [Column("TenSP")]
        [StringLength(150)]
        [Display(Name = "Tên sản phẩm")]
        public string TenSP { get; set; } = string.Empty;

        [Column("DonViTinh")]
        [StringLength(20)]
        [Display(Name = "Đơn vị tính")]
        public string? DonViTinh { get; set; }

        [Column("LoaiSP")]
        [StringLength(50)]
        [Display(Name = "Loại sản phẩm")]
        public string? LoaiSP { get; set; }

        [Column("MaDM")]
        [StringLength(20)]
        [Display(Name = "Danh mục")]
        public string? MaDM { get; set; }

        [Column("MaThue")]
        [StringLength(20)]
        [Display(Name = "Thuế")]
        public string? MaThue { get; set; }

        // Navigation properties
        [ForeignKey("MaDM")]
        public virtual DanhMuc? DanhMucNavigation { get; set; }

        [ForeignKey("MaThue")]
        public virtual Thue? ThueNavigation { get; set; }
    }
}