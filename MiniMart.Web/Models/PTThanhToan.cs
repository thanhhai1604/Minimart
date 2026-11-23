using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("PTTHANHTOAN")]
    public class PTThanhToan
    {
        [Key]
        [Column("MaPTTT")]
        [Required(ErrorMessage = "Mã phương thức thanh toán không được để trống.")]
        [StringLength(20)]
        [Display(Name = "Mã PTTT")]
        public string MaPTTT { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên phương thức thanh toán không được để trống.")]
        [Column("TenPTTT")]
        [StringLength(100)]
        [Display(Name = "Tên phương thức thanh toán")]
        public string TenPTTT { get; set; } = string.Empty;

        [Column("TrangThai")]
        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1; // 1: Sử dụng, 0: Không sử dụng
    }
}