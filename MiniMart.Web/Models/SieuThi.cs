using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("SIEUTHI")] 
    public class SieuThi
    {
        [Key]
        [Required(ErrorMessage = "Mã siêu thị không được để trống.")]
        [StringLength(50, ErrorMessage = "Mã siêu thị không được vượt quá 50 ký tự.")]
        [Display(Name = "Mã siêu thị")]
        public string MaST { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên siêu thị không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên siêu thị không được vượt quá 150 ký tự.")]
        [Display(Name = "Tên siêu thị")]
        public string TenST { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(250, ErrorMessage = "Địa chỉ không được vượt quá 250 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? SoDT { get; set; }

        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1;
    }
}