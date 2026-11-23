using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Web.Models
{
    [Table("CHITIETHOADON")]
    public class ChiTietHoaDon
    {
        [Key, Column("MaHD", Order = 0)]
        public string MaHD { get; set; } = string.Empty;

        [Key, Column("MaSP", Order = 1)]
        public string MaSP { get; set; } = string.Empty;

        [Column("SoLuong")]
        public int SoLuong { get; set; }

        [Column("DonGia")]
        public decimal DonGia { get; set; }

        [Column("ThanhTien")]
        public decimal ThanhTien { get; set; }
    }
}