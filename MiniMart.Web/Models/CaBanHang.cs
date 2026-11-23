using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("CABANHANG")]
    public class CaBanHang
    {
        [Key]
        [StringLength(50)]
        public string MaCa { get; set; } = null!;

        public DateTime? ThoiGianMo { get; set; } = DateTime.Now;

        public DateTime? ThoiGianDong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienDauCa { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienCuoiCa { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTienBan { get; set; } = 0;

        public int? TrangThai { get; set; } = 1;

        [StringLength(20)]
        public string? MaNV { get; set; }

        [StringLength(20)]
        public string? MaST { get; set; }

        [StringLength(20)]
        public string? MaPOS { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }
    }
}