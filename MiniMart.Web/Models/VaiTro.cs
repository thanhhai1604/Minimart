using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("VAITRO")]
    public class VaiTro
    {
        [Key]
        [Column("MaVaiTro")]
        [StringLength(50)]
        [Display(Name = "Mã vai trò")]
        public string MaVaiTro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [Column("TenVaiTro")]
        [StringLength(100)]
        [Display(Name = "Tên vai trò")]
        public string TenVaiTro { get; set; } = string.Empty;

        [Column("MoTa")]
        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        // Danh sách nhân viên thuộc vai trò này
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    }
}