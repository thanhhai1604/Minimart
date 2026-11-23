using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("CHITIETTHANHTOAN")]
    public class ChiTietThanhToan
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string MaHD { get; set; } = null!;

        [Key, Column(Order = 1)]
        [StringLength(20)]
        public string MaPTTT { get; set; } = null!; // Khớp chính xác với cột MaPTTT trong SQL

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienThanhToan { get; set; }

        [ForeignKey("MaHD")]
        public virtual HoaDon? HoaDon { get; set; }

        [ForeignKey("MaPTTT")]
        public virtual PTThanhToan? PTThanhToan { get; set; }
    }
}