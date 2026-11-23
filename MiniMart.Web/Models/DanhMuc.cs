using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("DANHMUC")] // Hoặc tên bảng của bạn
    public class DanhMuc
    {
        [Key]
        [Required(ErrorMessage = "Mã danh mục không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Mã danh mục")]
        public string MaDM { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(150)]
        [Display(Name = "Tên danh mục")]
        public string TenDanhMuc { get; set; } = string.Empty;

        // Bổ sung thuộc tính điều hướng này để liên kết với bảng SanPham
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}