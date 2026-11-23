using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("POS")]
    public class POS
    {
        // Khóa chính kép (MaPOS + MaST) được khai báo trong MiniMartDbContext.OnModelCreating:
        //   modelBuilder.Entity<POS>().HasKey(t => new { t.MaPOS, t.MaST });
        // EF Core không hỗ trợ khóa kép bằng thuộc tính [Key], nên ở đây không đặt [Key].

        [Column("MaPOS")]
        [Required(ErrorMessage = "Mã POS không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Mã POS")]
        public string MaPOS { get; set; } = string.Empty;

        [Column("MaST")]
        [Required(ErrorMessage = "Vui lòng chọn siêu thị trực thuộc.")]
        [Display(Name = "Mã siêu thị")]
        public string MaST { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên POS không được để trống.")]
        [StringLength(150)]
        [Display(Name = "Tên POS")]
        public string TenPOS { get; set; } = string.Empty;

        [Display(Name = "Trạng thái sử dụng")]
        [Column("TrangThaiSuDung")]
        public int TrangThai { get; set; } = 0; // 0: Chưa sử dụng, 1: Đã sử dụng

        [ForeignKey("MaST")]
        public virtual SieuThi? SieuThiNavigation { get; set; }
    }
}
