using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("NHACUNGCAP")]
    public class NhaCungCap
    {
        [Key]
        [Column("MaNCC")]
        [Required(ErrorMessage = "Mã nhà cung cấp không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Mã nhà cung cấp")]
        public string MaNCC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhà cung cấp không được để trống.")]
        [Column("TenNCC")]
        [StringLength(150)]
        [Display(Name = "Tên nhà cung cấp")]
        public string TenNCC { get; set; } = string.Empty;

        [Column("SoDT")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDT { get; set; }

        [Column("Email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        // Ánh xạ chính xác tên cột trong cơ sở dữ liệu PostgreSQL
        [Column("DiaChi")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Diachi { get; set; }
    }
}