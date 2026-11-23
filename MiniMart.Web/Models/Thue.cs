using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("THUE")]
    public class Thue
    {
        [Key]
        [Required(ErrorMessage = "Mã thuế không được để trống")]
        [StringLength(50)]
        [Display(Name = "Mã thuế")]
        public string MaThue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên thuế không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên thuế")]
        public string TenThue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thuế suất không được để trống")]
        [Range(0, 1, ErrorMessage = "Thuế suất phải nằm trong khoảng từ 0 đến 1 (ví dụ: 0.1 cho 10%)")]
        [Display(Name = "Thuế suất")]
        public decimal ThueSuat { get; set; }

        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } = 1; // Mặc định hoạt động (1)
    }
}